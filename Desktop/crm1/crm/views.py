"""
Представления для CRM системы управления магазином
"""
from django.shortcuts import redirect, get_object_or_404
from django.contrib.auth import login
from django.contrib.auth.forms import UserCreationForm, AuthenticationForm
from django.contrib.auth.views import LoginView
from django.contrib import messages
from django.views.generic import (
    ListView, DetailView, CreateView, UpdateView, DeleteView, TemplateView
)
from django.contrib.auth.mixins import LoginRequiredMixin
from django.urls import reverse_lazy, reverse
from django.utils import timezone
from django.db import transaction
from django.db.models import Sum, Count, Q, F
from datetime import datetime, timedelta
from decimal import Decimal
from django.http import JsonResponse
from django.core.mail import send_mail
from django.template.loader import render_to_string
from django.conf import settings

from .models import (
    Category, Supplier, Product, Customer, Order, OrderItem
)
from .forms import (
    CategoryForm, SupplierForm, ProductForm, CustomerForm,
    OrderForm, OrderItemFormSet
)


def _snapshot_from_items(items_queryset):
    """
    Вернуть словарь product_id -> суммарное количество по позициям.
    """
    snapshot = {}
    for item in items_queryset:
        if item.product_id:
            snapshot[item.product_id] = snapshot.get(item.product_id, 0) + (item.quantity or 0)
    return snapshot


def _snapshot_from_formset(formset):
    """
    Вернуть словарь product_id -> суммарное количество из formset.cleaned_data.
    Пропускаем удаленные/пустые строки.
    """
    snapshot = {}
    for form in formset.forms:
        if not hasattr(form, "cleaned_data"):
            continue
        if form.cleaned_data.get("DELETE"):
            continue
        product = form.cleaned_data.get("product")
        quantity = form.cleaned_data.get("quantity") or 0
        if product:
            snapshot[product.id] = snapshot.get(product.id, 0) + quantity
    return snapshot


def _calc_product_deltas(old_snapshot, new_snapshot):
    """
    old_snapshot/new_snapshot: dict product_id -> qty
    return: dict product_id -> delta (new - old)
    """
    deltas = {}
    for prod_id, qty in old_snapshot.items():
        deltas[prod_id] = deltas.get(prod_id, 0) - qty
    for prod_id, qty in new_snapshot.items():
        deltas[prod_id] = deltas.get(prod_id, 0) + qty
    return deltas


def ensure_stock_for_status_transition(order_status_old, order_status_new, old_snapshot, new_snapshot):
    """
    Проверить, хватает ли склада для перехода в статус completed.
    Бросает ValueError с текстом, если нехватает.
    """
    # Новый completed: проверяем, что весь новый объём доступен к списанию
    # Переход в completed впервые: нужно наличие new_snapshot целиком
    if order_status_old != "completed" and order_status_new == "completed":
        for prod_id, qty in new_snapshot.items():
            if qty <= 0:
                continue
            product = Product.objects.filter(pk=prod_id).first()
            if not product:
                continue
            if product.stock_quantity < qty:
                raise ValueError(
                    f'Недостаточно товара "{product.name}". Доступно: {product.stock_quantity}, нужно: {qty}'
                )
        return
    
    # Уже completed и правим количество: проверяем только увеличение (дельту)
    # Коррекция в completed -> completed: проверяем дельту (увеличение)
    if order_status_old == "completed" and order_status_new == "completed":
        deltas = _calc_product_deltas(old_snapshot, new_snapshot)
        for prod_id, delta in deltas.items():
            if delta <= 0:
                continue
            product = Product.objects.filter(pk=prod_id).first()
            if not product:
                continue
            if product.stock_quantity < delta:
                raise ValueError(
                    f'Недостаточно товара "{product.name}". Доступно: {product.stock_quantity}, нужно: {delta}'
                )
        return
    
    # Иные переходы (не требуют проверки склада)
    return


def apply_stock_for_status_transition(order_status_old, order_status_new, old_snapshot, new_snapshot):
    """
    Применить списание/возврат со склада в зависимости от смены статуса.
    - При переходе в completed: списываем дельту (new - old).
    - При уходе из completed: возвращаем дельту.
    - Если оба completed: корректируем по дельте (изменение qty/позиций).
    - Иначе ничего не делаем (pending/processing/cancelled без движения склада).
    """
    if order_status_old != "completed" and order_status_new == "completed":
        # Списываем весь новый снимок
        for prod_id, qty in new_snapshot.items():
            if qty == 0:
                continue
            product = Product.objects.filter(pk=prod_id).first()
            if not product:
                continue
            if product.stock_quantity < qty:
                raise ValueError(
                    f'Недостаточно товара "{product.name}". Доступно: {product.stock_quantity}, нужно: {qty}'
                )
            product.stock_quantity -= qty
            product.save()
    elif order_status_old == "completed" and order_status_new != "completed":
        # Возврат всего старого снимка
        for prod_id, qty in old_snapshot.items():
            if qty == 0:
                continue
            product = Product.objects.filter(pk=prod_id).first()
            if not product:
                continue
            product.stock_quantity += qty
            product.save()
    elif order_status_old == "completed" and order_status_new == "completed":
        # Корректировка по дельте
        deltas = _calc_product_deltas(old_snapshot, new_snapshot)
        for prod_id, delta in deltas.items():
            if delta == 0:
                continue
            product = Product.objects.filter(pk=prod_id).first()
            if not product:
                continue
            if delta > 0 and product.stock_quantity < delta:
                raise ValueError(
                    f'Недостаточно товара "{product.name}". Доступно: {product.stock_quantity}, нужно: {delta}'
                )
            product.stock_quantity -= delta
            product.save()
    # другие переходы: ничего не делаем


def send_order_status_email(order, old_status, new_status):
    """
    Отправить email клиенту об изменении статуса заказа
    """
    # Определяем email клиента
    customer_email = None
    customer_name = 'Клиент'
    
    if order.customer_email:
        customer_email = order.customer_email.strip()
        customer_name = order.customer_name or order.get_customer_display_name()
    elif order.customer and order.customer.email:
        customer_email = order.customer.email.strip()
        customer_name = order.customer.get_full_name()
    
    # Если email не указан, не отправляем письмо
    if not customer_email:
        import logging
        logger = logging.getLogger(__name__)
        logger.warning(f'Не удалось отправить email для заказа #{order.id}: email не указан')
        print(f'Предупреждение: Не указан email для заказа #{order.id}')
        return False
    
    # Получаем отображаемые названия статусов
    status_choices = dict(Order.STATUS_CHOICES)
    old_status_display = status_choices.get(old_status, old_status)
    new_status_display = status_choices.get(new_status, new_status)
    
    # Подготавливаем контекст для шаблона
    context = {
        'order': order,
        'customer_name': customer_name,
        'old_status': old_status,
        'new_status': new_status,
        'old_status_display': old_status_display,
        'new_status_display': new_status_display,
    }
    
    # Рендерим HTML и текстовую версии письма
    html_message = render_to_string('crm/email/order_status_changed.html', context)
    plain_message = render_to_string('crm/email/order_status_changed.txt', context)
    
    # Отправляем письмо
    try:
        result = send_mail(
            subject=f'Статус заказа #{order.id} изменен',
            message=plain_message,
            from_email=getattr(settings, 'DEFAULT_FROM_EMAIL', 'noreply@example.com'),
            recipient_list=[customer_email],
            html_message=html_message,
            fail_silently=False,  # Показываем ошибки для диагностики
        )
        if result:
            import logging
            logger = logging.getLogger(__name__)
            logger.info(f'Email успешно отправлен для заказа #{order.id} на {customer_email}')
            print(f'Email успешно отправлен для заказа #{order.id} на {customer_email}')
            return True
        else:
            import logging
            logger = logging.getLogger(__name__)
            logger.warning(f'Не удалось отправить email для заказа #{order.id} на {customer_email}')
            print(f'Предупреждение: Не удалось отправить email для заказа #{order.id} на {customer_email}')
            return False
    except Exception as e:
        # Логируем ошибку, но не прерываем работу приложения
        import logging
        logger = logging.getLogger(__name__)
        logger.error(f'Ошибка при отправке email для заказа #{order.id} на {customer_email}: {e}')
        # Выводим предупреждение в консоль для отладки
        print(f'Ошибка при отправке email для заказа #{order.id} на {customer_email}: {e}')
        import traceback
        traceback.print_exc()
        return False


def send_order_created_email(order):
    """
    Отправить email клиенту о создании нового заказа
    """
    # Определяем email клиента
    customer_email = None
    customer_name = 'Клиент'
    
    if order.customer_email:
        customer_email = order.customer_email.strip()
        customer_name = order.customer_name or order.get_customer_display_name()
    elif order.customer and order.customer.email:
        customer_email = order.customer.email.strip()
        customer_name = order.customer.get_full_name()
    
    # Если email не указан, не отправляем письмо
    if not customer_email:
        import logging
        logger = logging.getLogger(__name__)
        logger.warning(f'Не удалось отправить email о создании заказа #{order.id}: email не указан')
        return False
    
    # Получаем отображаемое название статуса
    status_choices = dict(Order.STATUS_CHOICES)
    status_display = status_choices.get(order.status, order.status)
    
    # Подготавливаем контекст для шаблона
    context = {
        'order': order,
        'customer_name': customer_name,
        'status_display': status_display,
    }
    
    # Рендерим HTML и текстовую версии письма
    html_message = render_to_string('crm/email/order_created.html', context)
    plain_message = render_to_string('crm/email/order_created.txt', context)
    
    # Отправляем письмо
    try:
        result = send_mail(
            subject=f'Ваш заказ #{order.id} создан',
            message=plain_message,
            from_email=getattr(settings, 'DEFAULT_FROM_EMAIL', 'noreply@example.com'),
            recipient_list=[customer_email],
            html_message=html_message,
            fail_silently=False,
        )
        if result:
            import logging
            logger = logging.getLogger(__name__)
            logger.info(f'Email о создании заказа успешно отправлен для заказа #{order.id} на {customer_email}')
            print(f'Email о создании заказа успешно отправлен для заказа #{order.id} на {customer_email}')
            return True
        else:
            import logging
            logger = logging.getLogger(__name__)
            logger.warning(f'Не удалось отправить email о создании заказа #{order.id} на {customer_email}')
            return False
    except Exception as e:
        # Логируем ошибку, но не прерываем работу приложения
        import logging
        logger = logging.getLogger(__name__)
        logger.error(f'Ошибка при отправке email о создании заказа #{order.id} на {customer_email}: {e}')
        print(f'Ошибка при отправке email о создании заказа #{order.id} на {customer_email}: {e}')
        import traceback
        traceback.print_exc()
        return False


# Authentication Views
class RegisterView(CreateView):
    """Регистрация нового пользователя"""
    form_class = UserCreationForm
    template_name = 'crm/register.html'
    success_url = reverse_lazy('dashboard')
    
    def get_context_data(self, **kwargs):
        context = super().get_context_data(**kwargs)
        # Добавляем CSS классы к полям формы
        if 'form' in context:
            form = context['form']
            for field_name, field in form.fields.items():
                field.widget.attrs.update({
                    'class': 'form-control',
                    'autocomplete': 'off' if field_name != 'password1' and field_name != 'password2' else 'new-password'
                })
                if field_name == 'username':
                    field.widget.attrs['autofocus'] = True
                    field.widget.attrs['autocomplete'] = 'username'
        return context
    
    def form_valid(self, form):
        response = super().form_valid(form)
        user = form.save()
        
        # Присваиваем права администратора новому пользователю
        user.is_staff = True
        user.is_superuser = True
        user.save()
        
        login(self.request, user)
        
        # Создаем начальную структуру CRM для нового пользователя
        try:
            self._initialize_crm_for_user(user)
        except Exception as e:
            import logging
            logger = logging.getLogger(__name__)
            logger.error(f'Ошибка при инициализации CRM для пользователя {user.username}: {e}')
        
        # Перенаправляем на страницу первого запуска
        return redirect('crm_setup')
    
    def _initialize_crm_for_user(self, user):
        """
        Создать начальную структуру CRM для нового пользователя
        """
        # Создаем базовые категории
        Category.objects.get_or_create(
            name='Общее',
            defaults={'description': 'Общая категория для товаров'}
        )
        Category.objects.get_or_create(
            name='Без категории',
            defaults={'description': 'Товары без категории'}
        )
    
    def form_invalid(self, form):
        messages.error(self.request, 'Пожалуйста, исправьте ошибки в форме.')
        return super().form_invalid(form)


class CustomLoginView(LoginView):
    """Вход в систему"""
    template_name = 'crm/login.html'
    authentication_form = AuthenticationForm
    redirect_authenticated_user = True
    
    def form_invalid(self, form):
        """Обработка ошибок входа с подробными сообщениями"""
        username = self.request.POST.get('username', '')
        # Проверяем, существует ли пользователь
        from django.contrib.auth.models import User
        try:
            user = User.objects.get(username=username)
            if not user.is_active:
                messages.error(self.request, 'Ваш аккаунт неактивен. Обратитесь к администратору.')
            else:
                messages.error(self.request, 'Неверный пароль. Проверьте правильность ввода.')
        except User.DoesNotExist:
            messages.error(self.request, f'Пользователь "{username}" не найден.')
        except Exception as e:
            messages.error(self.request, f'Ошибка входа: {str(e)}')
        return super().form_invalid(form)


# CRM Setup (First Launch)
class CRMSetupView(LoginRequiredMixin, TemplateView):
    """Страница первого запуска CRM для нового пользователя"""
    template_name = 'crm/crm_setup.html'
    
    def get_context_data(self, **kwargs):
        context = super().get_context_data(**kwargs)
        # Статистика для проверки, есть ли уже данные
        context['has_categories'] = Category.objects.exists()
        context['has_products'] = Product.objects.exists()
        context['has_customers'] = Customer.objects.exists()
        context['has_orders'] = Order.objects.exists()
        context['has_suppliers'] = Supplier.objects.exists()
        return context


# Dashboard
class DashboardView(LoginRequiredMixin, TemplateView):
    """Главная страница - дашборд магазина"""
    template_name = 'crm/dashboard.html'
    
    def get_context_data(self, **kwargs):
        context = super().get_context_data(**kwargs)
        
        # Общая статистика
        today = timezone.now().date()
        this_month_start = today.replace(day=1)
        
        # Продажи
        total_orders = Order.objects.count()
        today_orders = Order.objects.filter(created_at__date=today).count()
        month_orders = Order.objects.filter(created_at__date__gte=this_month_start).count()
        
        # Выручка
        total_revenue = Order.objects.filter(
            payment_status='paid'
        ).aggregate(Sum('total_amount'))['total_amount__sum'] or Decimal('0')
        
        today_revenue = Order.objects.filter(
            created_at__date=today,
            payment_status='paid'
        ).aggregate(Sum('total_amount'))['total_amount__sum'] or Decimal('0')
        
        month_revenue = Order.objects.filter(
            created_at__date__gte=this_month_start,
            payment_status='paid'
        ).aggregate(Sum('total_amount'))['total_amount__sum'] or Decimal('0')
        
        # Товары
        total_products = Product.objects.count()
        low_stock_products = Product.objects.filter(
            stock_quantity__lte=F('min_stock_level')
        ).count()
        
        # Клиенты
        total_customers = Customer.objects.count()
        new_customers_today = Customer.objects.filter(created_at__date=today).count()
        
        # Последние заказы
        recent_orders = Order.objects.select_related('customer').order_by('-created_at')[:10]
        
        # Товары с низким остатком
        low_stock = Product.objects.filter(
            stock_quantity__lte=F('min_stock_level')
        ).order_by('stock_quantity')[:10]
        
        # Топ товары по продажам
        top_products = Product.objects.annotate(
            total_sold=Sum('order_items__quantity')
        ).filter(total_sold__gt=0).order_by('-total_sold')[:10]
        
        context.update({
            'total_orders': total_orders,
            'today_orders': today_orders,
            'month_orders': month_orders,
            'total_revenue': total_revenue,
            'today_revenue': today_revenue,
            'month_revenue': month_revenue,
            'total_products': total_products,
            'low_stock_products': low_stock_products,
            'total_customers': total_customers,
            'new_customers_today': new_customers_today,
            'recent_orders': recent_orders,
            'low_stock': low_stock,
            'top_products': top_products,
        })
        
        return context


class AnalyticsView(LoginRequiredMixin, TemplateView):
    """Отчетность по заказам и товарам"""
    template_name = 'crm/analytics.html'
    
    def get_context_data(self, **kwargs):
        context = super().get_context_data(**kwargs)
        
        today = timezone.now().date()
        month_start = today.replace(day=1)
        
        # Общие показатели заказов
        orders_qs = Order.objects.all()
        context['total_orders'] = orders_qs.count()
        context['completed_orders'] = orders_qs.filter(status='completed').count()
        context['cancelled_orders'] = orders_qs.filter(status='cancelled').count()
        context['paid_orders'] = orders_qs.filter(payment_status='paid').count()
        
        # Выручка
        paid_qs = orders_qs.filter(payment_status='paid')
        context['revenue_total'] = paid_qs.aggregate(Sum('total_amount'))['total_amount__sum'] or Decimal('0')
        context['revenue_month'] = paid_qs.filter(created_at__date__gte=month_start).aggregate(Sum('total_amount'))['total_amount__sum'] or Decimal('0')
        context['revenue_today'] = paid_qs.filter(created_at__date=today).aggregate(Sum('total_amount'))['total_amount__sum'] or Decimal('0')
        
        # Разбивка по статусам и оплате
        context['orders_by_status'] = orders_qs.values('status').annotate(
            count=Count('id'),
            total=Sum('total_amount')
        )
        context['orders_by_payment'] = orders_qs.values('payment_status').annotate(
            count=Count('id'),
            total=Sum('total_amount')
        )
        
        # Топ и рисковые товары
        context['top_products'] = Product.objects.annotate(
            total_sold=Sum('order_items__quantity')
        ).filter(total_sold__gt=0).order_by('-total_sold')[:10]
        
        context['low_stock'] = Product.objects.filter(
            stock_quantity__lte=F('min_stock_level')
        ).order_by('stock_quantity')[:10]
        
        # Последние заказы
        context['recent_orders'] = orders_qs.select_related('customer').order_by('-created_at')[:10]
        
        return context


# Category Views
class CategoryListView(LoginRequiredMixin, ListView):
    """Список категорий"""
    model = Category
    template_name = 'crm/category_list.html'
    context_object_name = 'categories'
    paginate_by = 20
    
    def get_queryset(self):
        """Получить список категорий с поиском"""
        queryset = Category.objects.all()
        
        search = self.request.GET.get('search', '')
        if search:
            queryset = queryset.filter(
                Q(name__icontains=search) |
                Q(description__icontains=search)
            )
        
        return queryset.order_by('name')
    
    def get_context_data(self, **kwargs):
        """Добавить поисковый запрос в контекст"""
        context = super().get_context_data(**kwargs)
        context['search_query'] = self.request.GET.get('search', '')
        return context


class CategoryCreateView(LoginRequiredMixin, CreateView):
    """Создание категории"""
    model = Category
    form_class = CategoryForm
    template_name = 'crm/category_form.html'
    success_url = reverse_lazy('category_list')
    
    def form_valid(self, form):
        messages.success(self.request, 'Категория успешно создана!')
        return super().form_valid(form)


class CategoryUpdateView(LoginRequiredMixin, UpdateView):
    """Редактирование категории"""
    model = Category
    form_class = CategoryForm
    template_name = 'crm/category_form.html'
    success_url = reverse_lazy('category_list')
    
    def form_valid(self, form):
        messages.success(self.request, 'Категория успешно обновлена!')
        return super().form_valid(form)


class CategoryDeleteView(LoginRequiredMixin, DeleteView):
    """Удаление категории"""
    model = Category
    template_name = 'crm/category_confirm_delete.html'
    success_url = reverse_lazy('category_list')
    
    def delete(self, request, *args, **kwargs):
        messages.success(request, 'Категория успешно удалена!')
        return super().delete(request, *args, **kwargs)


# Supplier Views
class SupplierListView(LoginRequiredMixin, ListView):
    """Список поставщиков"""
    model = Supplier
    template_name = 'crm/supplier_list.html'
    context_object_name = 'suppliers'
    paginate_by = 20
    
    def get_queryset(self):
        query = self.request.GET.get('search', '')
        if query:
            return Supplier.objects.filter(
                Q(name__icontains=query) |
                Q(contact_person__icontains=query) |
                Q(phone__icontains=query)
            )
        return Supplier.objects.all()


class SupplierDetailView(LoginRequiredMixin, DetailView):
    """Детали поставщика"""
    model = Supplier
    template_name = 'crm/supplier_detail.html'
    context_object_name = 'supplier'
    
    def get_context_data(self, **kwargs):
        context = super().get_context_data(**kwargs)
        supplier = self.get_object()
        context['products'] = supplier.products.all()[:20]
        return context


class SupplierCreateView(LoginRequiredMixin, CreateView):
    """Создание поставщика"""
    model = Supplier
    form_class = SupplierForm
    template_name = 'crm/supplier_form.html'
    
    def form_valid(self, form):
        messages.success(self.request, 'Поставщик успешно создан!')
        return super().form_valid(form)
    
    def get_success_url(self):
        return reverse('supplier_detail', kwargs={'pk': self.object.pk})


class SupplierUpdateView(LoginRequiredMixin, UpdateView):
    """Редактирование поставщика"""
    model = Supplier
    form_class = SupplierForm
    template_name = 'crm/supplier_form.html'
    
    def form_valid(self, form):
        messages.success(self.request, 'Поставщик успешно обновлен!')
        return super().form_valid(form)
    
    def get_success_url(self):
        return reverse('supplier_detail', kwargs={'pk': self.object.pk})


class SupplierDeleteView(LoginRequiredMixin, DeleteView):
    """Удаление поставщика"""
    model = Supplier
    template_name = 'crm/supplier_confirm_delete.html'
    success_url = reverse_lazy('supplier_list')
    
    def delete(self, request, *args, **kwargs):
        messages.success(request, 'Поставщик успешно удален!')
        return super().delete(request, *args, **kwargs)


# Product Views
class ProductListView(LoginRequiredMixin, ListView):
    """Список товаров"""
    model = Product
    template_name = 'crm/product_list.html'
    context_object_name = 'products'
    paginate_by = 20
    
    def get_queryset(self):
        """Получить список товаров с поиском и фильтрами"""
        queryset = Product.objects.select_related('category', 'supplier')
        
        search = self.request.GET.get('search', '')
        if search:
            queryset = queryset.filter(
                Q(name__icontains=search) |
                Q(description__icontains=search) |
                Q(sku__icontains=search) |
                Q(barcode__icontains=search) |
                Q(category__name__icontains=search)
            )
        
        category_id = self.request.GET.get('category', '')
        if category_id:
            queryset = queryset.filter(category_id=category_id)
        
        low_stock = self.request.GET.get('low_stock', '')
        if low_stock:
            queryset = queryset.filter(
                stock_quantity__lte=F('min_stock_level')
            )
        
        return queryset.order_by('name')
    
    def get_context_data(self, **kwargs):
        context = super().get_context_data(**kwargs)
        context['categories'] = Category.objects.all()
        context['search_query'] = self.request.GET.get('search', '')
        context['category_filter'] = self.request.GET.get('category', '')
        context['low_stock_filter'] = self.request.GET.get('low_stock', '')
        return context


class ProductDetailView(LoginRequiredMixin, DetailView):
    """Детали товара"""
    model = Product
    template_name = 'crm/product_detail.html'
    context_object_name = 'product'
    
    def get_context_data(self, **kwargs):
        context = super().get_context_data(**kwargs)
        product = self.get_object()
        context['order_items'] = product.order_items.select_related('order').order_by('-order__created_at')[:20]
        return context


class ProductCreateView(LoginRequiredMixin, CreateView):
    """Создание товара"""
    model = Product
    form_class = ProductForm
    template_name = 'crm/product_form.html'
    
    def form_valid(self, form):
        product = form.save(commit=False)
        product.created_by = self.request.user
        product.save()
        messages.success(self.request, 'Товар успешно создан!')
        return redirect('product_detail', pk=product.pk)


class ProductUpdateView(LoginRequiredMixin, UpdateView):
    """Редактирование товара"""
    model = Product
    form_class = ProductForm
    template_name = 'crm/product_form.html'
    
    def form_valid(self, form):
        messages.success(self.request, 'Товар успешно обновлен!')
        return super().form_valid(form)
    
    def get_success_url(self):
        return reverse('product_detail', kwargs={'pk': self.object.pk})


class ProductDeleteView(LoginRequiredMixin, DeleteView):
    """Удаление товара"""
    model = Product
    template_name = 'crm/product_confirm_delete.html'
    success_url = reverse_lazy('product_list')
    
    def delete(self, request, *args, **kwargs):
        messages.success(request, 'Товар успешно удален!')
        return super().delete(request, *args, **kwargs)


# Customer Views
class CustomerListView(LoginRequiredMixin, ListView):
    """Список клиентов"""
    model = Customer
    template_name = 'crm/customer_list.html'
    context_object_name = 'customers'
    paginate_by = 20
    
    def get_queryset(self):
        query = self.request.GET.get('search', '')
        if query:
            return Customer.objects.filter(
                Q(first_name__icontains=query) |
                Q(last_name__icontains=query) |
                Q(email__icontains=query) |
                Q(phone__icontains=query)
            )
        return Customer.objects.all()
    
    def get_context_data(self, **kwargs):
        context = super().get_context_data(**kwargs)
        context['search_query'] = self.request.GET.get('search', '')
        return context


class CustomerDetailView(LoginRequiredMixin, DetailView):
    """Детали клиента"""
    model = Customer
    template_name = 'crm/customer_detail.html'
    context_object_name = 'customer'
    
    def get_context_data(self, **kwargs):
        context = super().get_context_data(**kwargs)
        customer = self.get_object()
        context['orders'] = customer.orders.all().order_by('-created_at')[:20]
        return context


class CustomerCreateView(LoginRequiredMixin, CreateView):
    """Создание клиента"""
    model = Customer
    form_class = CustomerForm
    template_name = 'crm/customer_form.html'
    
    def form_valid(self, form):
        messages.success(self.request, 'Клиент успешно создан!')
        return super().form_valid(form)
    
    def get_success_url(self):
        return reverse('customer_detail', kwargs={'pk': self.object.pk})


class CustomerUpdateView(LoginRequiredMixin, UpdateView):
    """Редактирование клиента"""
    model = Customer
    form_class = CustomerForm
    template_name = 'crm/customer_form.html'
    
    def form_valid(self, form):
        messages.success(self.request, 'Клиент успешно обновлен!')
        return super().form_valid(form)
    
    def get_success_url(self):
        return reverse('customer_detail', kwargs={'pk': self.object.pk})


class CustomerDeleteView(LoginRequiredMixin, DeleteView):
    """Удаление клиента"""
    model = Customer
    template_name = 'crm/customer_confirm_delete.html'
    success_url = reverse_lazy('customer_list')
    
    def delete(self, request, *args, **kwargs):
        messages.success(request, 'Клиент успешно удален!')
        return super().delete(request, *args, **kwargs)


# Order Views
class OrderListView(LoginRequiredMixin, ListView):
    """Список заказов"""
    model = Order
    template_name = 'crm/order_list.html'
    context_object_name = 'orders'
    paginate_by = 20
    
    def get_queryset(self):
        queryset = Order.objects.select_related('customer').prefetch_related('items')
        
        status = self.request.GET.get('status', '')
        if status:
            queryset = queryset.filter(status=status)
        
        payment_status = self.request.GET.get('payment_status', '')
        if payment_status:
            queryset = queryset.filter(payment_status=payment_status)
        
        date_from = self.request.GET.get('date_from', '')
        date_to = self.request.GET.get('date_to', '')
        
        if date_from:
            queryset = queryset.filter(created_at__date__gte=date_from)
        if date_to:
            queryset = queryset.filter(created_at__date__lte=date_to)
        
        return queryset.order_by('-created_at')
    
    def get_context_data(self, **kwargs):
        context = super().get_context_data(**kwargs)
        context['status_filter'] = self.request.GET.get('status', '')
        context['payment_status_filter'] = self.request.GET.get('payment_status', '')
        context['date_from'] = self.request.GET.get('date_from', '')
        context['date_to'] = self.request.GET.get('date_to', '')
        return context


class OrderDetailView(LoginRequiredMixin, DetailView):
    """Детали заказа"""
    model = Order
    template_name = 'crm/order_detail.html'
    context_object_name = 'order'
    
    def get_context_data(self, **kwargs):
        context = super().get_context_data(**kwargs)
        order = self.get_object()
        context['items'] = order.items.select_related('product').all()
        return context


class OrderCreateView(LoginRequiredMixin, CreateView):
    """Создание заказа"""
    model = Order
    form_class = OrderForm
    template_name = 'crm/order_form.html'
    
    def get_context_data(self, **kwargs):
        context = super().get_context_data(**kwargs)
        if self.request.POST:
            context['formset'] = OrderItemFormSet(self.request.POST)
        else:
            context['formset'] = OrderItemFormSet()
        return context
    
    def form_valid(self, form):
        context = self.get_context_data()
        formset = context['formset']
        
        # Проверяем валидность formset
        if not formset.is_valid():
            # Если formset не валиден, показываем форму с ошибками
            return self.render_to_response(self.get_context_data(form=form, formset=formset))
        
        if formset.is_valid():
            with transaction.atomic():
                order = form.save(commit=False)
                order.created_by = self.request.user
                old_status = 'pending'  # новый заказ ещё не завершен, поэтому склад пока не трогаем
                old_snapshot = {}
                
                # Проверка: нельзя завершить заказ, если он не оплачен
                if order.status == 'completed' and order.payment_status != 'paid':
                    payment_status_choices = dict(Order.PAYMENT_STATUS_CHOICES)
                    payment_status_display = payment_status_choices.get(order.payment_status, order.payment_status)
                    form.add_error(
                        'status',
                        f'Нельзя завершить заказ, если он не оплачен. '
                        f'Текущий статус оплаты: "{payment_status_display}". '
                        f'Пожалуйста, сначала установите статус оплаты на "Оплачено" или выберите другой статус заказа.'
                    )
                    return self.render_to_response(self.get_context_data(form=form, formset=formset))
                
                # Проверим склад, если целевой статус completed
                try:
                    new_snapshot = _snapshot_from_formset(formset)
                    ensure_stock_for_status_transition(old_status, order.status, old_snapshot, new_snapshot)
                except ValueError as e:
                    form.add_error(None, str(e))
                    return self.render_to_response(self.get_context_data(form=form, formset=formset))
                
                order.save()
                
                formset.instance = order
                formset.save()
                
                # Автоматически заполнить цену из товара, если цена не указана
                for item in order.items.all():
                    if item.product:
                        # Проверяем, что цена не указана или равна 0
                        from decimal import Decimal
                        if not item.price or item.price == Decimal('0') or item.price == 0:
                            if item.product.selling_price and item.product.selling_price > 0:
                                item.price = item.product.selling_price
                                item.save()
                
                # Движение по складу в зависимости от статуса (только при completed)
                new_snapshot = _snapshot_from_items(order.items.all())
                try:
                    apply_stock_for_status_transition(old_status, order.status, {}, new_snapshot)
                except ValueError as e:
                    form.add_error(None, str(e))
                    transaction.set_rollback(True)
                    return self.render_to_response(self.get_context_data(form=form, formset=formset))
                
                # Обновить статистику клиента
                if order.customer:
                    order.customer.total_orders += 1
                    order.customer.total_spent += order.total_amount
                    order.customer.save()
                
                order.calculate_total()
                
                # Отправить email клиенту о создании заказа
                customer_email = order.customer_email or (order.customer.email if order.customer else None)
                if customer_email:
                    try:
                        result = send_order_created_email(order)
                        if result:
                            messages.success(
                                self.request, 
                                f'Заказ #{order.id} успешно создан! Email отправлен клиенту на {customer_email}'
                            )
                        else:
                            messages.warning(
                                self.request, 
                                f'Заказ #{order.id} создан, но не удалось отправить email клиенту на {customer_email}'
                            )
                    except Exception as e:
                        messages.warning(
                            self.request, 
                            f'Заказ #{order.id} создан, но произошла ошибка при отправке email: {str(e)}'
                        )
                else:
                    messages.success(
                        self.request, 
                        f'Заказ #{order.id} успешно создан! Email не отправлен: адрес не указан.'
                    )
                
                return redirect('order_detail', pk=order.pk)
        else:
            return self.render_to_response(context)
    
    def get_success_url(self):
        return reverse('order_detail', kwargs={'pk': self.object.pk})


class OrderUpdateView(LoginRequiredMixin, UpdateView):
    """Редактирование заказа"""
    model = Order
    form_class = OrderForm
    template_name = 'crm/order_form.html'
    
    def get_context_data(self, **kwargs):
        context = super().get_context_data(**kwargs)
        if self.request.POST:
            context['formset'] = OrderItemFormSet(self.request.POST, instance=self.object)
        else:
            context['formset'] = OrderItemFormSet(instance=self.object)
        context['original_status'] = self.object.status
        return context
    
    def form_valid(self, form):
        context = self.get_context_data()
        formset = context['formset']
        
        # Проверяем валидность formset
        if not formset.is_valid():
            # Если formset не валиден, показываем форму с ошибками
            return self.render_to_response(self.get_context_data(form=form, formset=formset))
        
        if formset.is_valid():
            with transaction.atomic():
                # КРИТИЧЕСКИ ВАЖНО: Перезагружаем объект из базы ДО получения старого статуса
                # Это гарантирует, что мы получим актуальное значение из БД, а не из кэша
                order_pk = self.object.pk
                old_order = Order.objects.get(pk=order_pk)  # Свежая копия из БД
                old_status = old_order.status  # Статус из базы данных ДО сохранения формы
                
                # КРИТИЧЕСКИ ВАЖНО: Получаем новый статус из POST напрямую
                # Это гарантирует, что мы получим то значение, которое пользователь РЕАЛЬНО выбрал в форме
                # cleaned_data может содержать старое значение из instance, поэтому не доверяем ему полностью
                post_status = self.request.POST.get('status')
                cleaned_status = form.cleaned_data.get('status')
                
                # Приоритет: POST данные (что пользователь отправил) > cleaned_data
                new_status = post_status if post_status else cleaned_status
                
                # Критическая диагностика
                print(f'='*70)
                print(f'DEBUG OrderUpdateView: Заказ #{self.object.id}')
                print(f'  old_status (из объекта) = "{old_status}"')
                print(f'  POST["status"] (что отправил пользователь) = "{post_status}"')
                print(f'  form.cleaned_data["status"] = "{cleaned_status}"')
                print(f'  new_status (используемое, приоритет POST) = "{new_status}"')
                print(f'  Сравнение: "{old_status}" != "{new_status}" = {old_status != new_status}')
                if old_status == new_status:
                    print(f'  ⚠️  ВНИМАНИЕ: Статусы совпадают!')
                    print(f'     Возможно, пользователь выбрал тот же статус, что и был.')
                print(f'='*70)
                
                old_snapshot = _snapshot_from_items(self.object.items.all())
                
                # Проверка: нельзя завершить заказ, если он не оплачен
                payment_status = form.cleaned_data.get('payment_status') or old_order.payment_status
                if new_status == 'completed' and payment_status != 'paid':
                    payment_status_choices = dict(Order.PAYMENT_STATUS_CHOICES)
                    payment_status_display = payment_status_choices.get(payment_status, payment_status)
                    form.add_error(
                        'status',
                        f'Нельзя завершить заказ, если он не оплачен. '
                        f'Текущий статус оплаты: "{payment_status_display}". '
                        f'Пожалуйста, сначала измените статус оплаты на "Оплачено" или выберите другой статус заказа.'
                    )
                    return self.render_to_response(self.get_context_data(form=form, formset=formset))
                
                # Проверим склад, если целевой статус completed
                try:
                    new_snapshot_form = _snapshot_from_formset(formset)
                    ensure_stock_for_status_transition(old_status, new_status, old_snapshot, new_snapshot_form)
                except ValueError as e:
                    form.add_error(None, str(e))
                    return self.render_to_response(self.get_context_data(form=form, formset=formset))
                
                order = form.save()
                formset.save()
                
                # Обновляем объект из базы, чтобы получить актуальный статус
                order.refresh_from_db()
                
                # Автоматически заполнить цену из товара, если цена не указана
                for item in order.items.all():
                    if item.product:
                        # Проверяем, что цена не указана или равна 0
                        if not item.price or item.price == Decimal('0') or item.price == 0:
                            if item.product.selling_price and item.product.selling_price > 0:
                                item.price = item.product.selling_price
                                item.save()
                
                # Пересчитать склад с учетом изменений (добавления, удаления, изменения qty) и статуса
                new_snapshot = _snapshot_from_items(order.items.all())
                try:
                    apply_stock_for_status_transition(old_status, order.status, old_snapshot, new_snapshot)
                except ValueError as e:
                    form.add_error(None, str(e))
                    transaction.set_rollback(True)
                    return self.render_to_response(self.get_context_data(form=form, formset=formset))
                
                order.calculate_total()
                
                # Отправить email клиенту, если статус изменился
                # Используем new_status из формы, а не order.status после refresh_from_db()
                # так как order.status может быть уже обновлен
                actual_new_status = new_status  # статус из формы
                status_choices = dict(Order.STATUS_CHOICES)
                old_status_display = status_choices.get(old_status, old_status)
                new_status_display = status_choices.get(actual_new_status, actual_new_status)
                
                if old_status != actual_new_status:
                    # Статус изменился - отправляем email
                    print(f'DEBUG OrderUpdateView: Статус заказа #{order.id} изменился с "{old_status}" на "{actual_new_status}"')
                    customer_email = order.customer_email or (order.customer.email if order.customer else None)
                    
                    if customer_email:
                        try:
                            result = send_order_status_email(order, old_status, actual_new_status)
                            if result:
                                messages.success(
                                    self.request, 
                                    f'Заказ #{order.id} обновлен! Статус изменен с "{old_status_display}" на "{new_status_display}". '
                                    f'Email отправлен на {customer_email}'
                                )
                            else:
                                messages.warning(
                                    self.request, 
                                    f'Заказ #{order.id} обновлен (статус: {old_status_display} → {new_status_display}), '
                                    f'но не удалось отправить email на {customer_email}'
                                )
                        except Exception as e:
                            messages.warning(
                                self.request, 
                                f'Заказ обновлен (статус: {old_status_display} → {new_status_display}), '
                                f'но произошла ошибка при отправке email: {str(e)}'
                            )
                    else:
                        messages.success(
                            self.request, 
                            f'Заказ #{order.id} обновлен! Статус изменен с "{old_status_display}" на "{new_status_display}". '
                            'Email не отправлен: адрес не указан.'
                        )
                else:
                    # Статус не изменился - показываем детальную информацию
                    print(f'DEBUG OrderUpdateView: Статус заказа #{order.id} не изменился.')
                    print(f'  old_status="{old_status}", actual_new_status="{actual_new_status}"')
                    print(f'  POST status = "{self.request.POST.get("status", "не найден")}"')
                    messages.warning(
                        self.request, 
                        f'Заказ #{order.id} обновлен! Статус не изменился (остался "{old_status_display}"). '
                        f'[DEBUG: было="{old_status}", стало="{actual_new_status}", POST="{self.request.POST.get("status", "не найден")}"] '
                        'Email не отправлен. Убедитесь, что вы выбрали ДРУГОЙ статус в выпадающем списке перед сохранением.'
                    )
                
                return redirect('order_detail', pk=order.pk)
        else:
            return self.render_to_response(context)


class OrderDeleteView(LoginRequiredMixin, DeleteView):
    """Удаление заказа"""
    model = Order
    template_name = 'crm/order_confirm_delete.html'
    success_url = reverse_lazy('order_list')
    
    def delete(self, request, *args, **kwargs):
        order = self.get_object()
        
        # Вернуть товары на склад
        for item in order.items.all():
            item.product.stock_quantity += item.quantity
            item.product.save()
        
        # Обновить статистику клиента
        if order.customer:
            order.customer.total_orders = max(0, order.customer.total_orders - 1)
            order.customer.total_spent = max(Decimal('0'), order.customer.total_spent - order.total_amount)
            order.customer.save()
        
        messages.success(request, 'Заказ успешно удален!')
        return super().delete(request, *args, **kwargs)


# AJAX Views
class GetProductPriceView(LoginRequiredMixin, TemplateView):
    """AJAX endpoint для получения цены товара"""
    
    def get(self, request, *args, **kwargs):
        product_id = request.GET.get('product_id')
        if not product_id:
            return JsonResponse({'error': 'Product ID is required'}, status=400)
        
        try:
            product = Product.objects.get(pk=product_id, is_active=True)
            return JsonResponse({
                'selling_price': str(product.selling_price),
                'purchase_price': str(product.purchase_price),
                'stock_quantity': product.stock_quantity,
                'name': product.name
            })
        except Product.DoesNotExist:
            return JsonResponse({'error': 'Product not found'}, status=404)


class SearchCustomersView(LoginRequiredMixin, TemplateView):
    """AJAX endpoint для поиска клиентов"""
    
    def get(self, request, *args, **kwargs):
        """Поиск клиентов по запросу"""
        query = request.GET.get('q', '').strip()
        
        if not query or len(query) < 2:
            return JsonResponse({'customers': []})
        
        # Нормализуем запрос для регистронезависимого поиска
        query_lower = query.lower()
        
        # Получаем всех клиентов
        all_customers = Customer.objects.all()
        
        # Фильтруем в Python для правильной работы с кириллицей
        matching_customers = []
        for customer in all_customers:
            # Проверяем имя
            if customer.first_name and query_lower in customer.first_name.lower():
                matching_customers.append(customer)
                continue
            # Проверяем фамилию
            if customer.last_name and query_lower in customer.last_name.lower():
                matching_customers.append(customer)
                continue
            # Проверяем email
            if customer.email and query_lower in customer.email.lower():
                matching_customers.append(customer)
                continue
            # Проверяем телефон
            if customer.phone and query_lower in customer.phone.lower():
                matching_customers.append(customer)
                continue
        
        # Ограничиваем 10 результатами
        customers = matching_customers[:10]
        
        results = []
        for customer in customers:
            results.append({
                'id': customer.id,
                'name': customer.get_full_name(),
                'first_name': customer.first_name,
                'last_name': customer.last_name,
                'email': customer.email or '',
                'phone': customer.phone or '',
            })
        
        return JsonResponse({'customers': results})


class SearchProductsView(LoginRequiredMixin, TemplateView):
    """AJAX endpoint для поиска товаров"""
    
    def get(self, request, *args, **kwargs):
        """Поиск товаров по запросу"""
        query = request.GET.get('q', '').strip()
        
        if not query or len(query) < 2:
            return JsonResponse({'products': []})
        
        # Нормализуем запрос для регистронезависимого поиска
        query_lower = query.lower()
        
        # Получаем все активные товары
        all_products = Product.objects.filter(
            is_active=True
        ).select_related('category')
        
        # Фильтруем в Python для правильной работы с кириллицей
        matching_products = []
        for product in all_products:
            # Проверяем название
            if query_lower in (product.name or '').lower():
                matching_products.append(product)
                continue
            # Проверяем описание
            if product.description and query_lower in product.description.lower():
                matching_products.append(product)
                continue
            # Проверяем артикул
            if product.sku and query_lower in product.sku.lower():
                matching_products.append(product)
                continue
            # Проверяем штрих-код
            if product.barcode and query_lower in product.barcode.lower():
                matching_products.append(product)
                continue
            # Проверяем категорию
            if product.category and query_lower in product.category.name.lower():
                matching_products.append(product)
                continue
        
        # Ограничиваем 20 результатами
        products = matching_products[:20]
        
        results = []
        for product in products:
            results.append({
                'id': product.id,
                'name': product.name,
                'sku': product.sku or '',
                'barcode': product.barcode or '',
                'selling_price': str(product.selling_price),
                'stock_quantity': product.stock_quantity,
                'category': product.category.name if product.category else '',
            })
        
        return JsonResponse({'products': results})


class CheckOrderStockView(LoginRequiredMixin, TemplateView):
    """AJAX endpoint для проверки наличия товаров на складе для заказа"""
    
    def post(self, request, *args, **kwargs):
        """Проверить наличие товаров и списать их со склада"""
        order_id = request.POST.get('order_id')
        if not order_id:
            return JsonResponse({'error': 'Order ID is required'}, status=400)
        
        try:
            order = Order.objects.get(pk=order_id)
        except Order.DoesNotExist:
            return JsonResponse({'error': 'Order not found'}, status=404)
        
        # Проверяем, не завершен ли уже заказ (чтобы избежать повторного списания)
        # Если заказ уже завершен, товары уже были списаны при сохранении
        if order.status == 'completed':
            return JsonResponse({
                'success': False,
                'error': 'Заказ уже завершен. Товары были автоматически списаны при завершении заказа.'
            }, status=400)
        
        # Получаем снимок товаров в заказе
        items_snapshot = _snapshot_from_items(order.items.all())
        
        # Проверяем наличие товаров на складе
        insufficient_items = []
        for product_id, qty in items_snapshot.items():
            if qty <= 0:
                continue
            try:
                product = Product.objects.get(pk=product_id)
                if product.stock_quantity < qty:
                    insufficient_items.append({
                        'product_name': product.name,
                        'available': product.stock_quantity,
                        'needed': qty
                    })
            except Product.DoesNotExist:
                insufficient_items.append({
                    'product_name': f'Товар #{product_id}',
                    'available': 0,
                    'needed': qty
                })
        
        if insufficient_items:
            return JsonResponse({
                'success': False,
                'error': 'Недостаточно товаров на складе',
                'insufficient_items': insufficient_items
            }, status=400)
        
        # Если все товары есть, списываем их со склада
        try:
            with transaction.atomic():
                for product_id, qty in items_snapshot.items():
                    if qty <= 0:
                        continue
                    product = Product.objects.select_for_update().get(pk=product_id)
                    if product.stock_quantity < qty:
                        return JsonResponse({
                            'success': False,
                            'error': f'Недостаточно товара "{product.name}" на складе'
                        }, status=400)
                    product.stock_quantity -= qty
                    product.save()
            
            return JsonResponse({
                'success': True,
                'message': 'Товары успешно списаны со склада'
            })
        except Exception as e:
            return JsonResponse({
                'success': False,
                'error': str(e)
            }, status=500)
