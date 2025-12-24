"""
Модели для CRM системы управления магазином
"""
from django.db import models
from django.contrib.auth.models import User
from django.urls import reverse
from django.utils import timezone
from django.core.validators import MinValueValidator


class Category(models.Model):
    """Категория товаров"""
    name = models.CharField(max_length=100, verbose_name='Название категории', unique=True)
    description = models.TextField(verbose_name='Описание', blank=True)
    created_at = models.DateTimeField(auto_now_add=True, verbose_name='Создана')
    
    class Meta:
        verbose_name = 'Категория'
        verbose_name_plural = 'Категории'
        ordering = ['name']
    
    def __str__(self):
        return self.name
    
    def get_absolute_url(self):
        return reverse('category_detail', kwargs={'pk': self.pk})


class Supplier(models.Model):
    """Поставщик товаров"""
    name = models.CharField(max_length=200, verbose_name='Название поставщика')
    contact_person = models.CharField(max_length=100, verbose_name='Контактное лицо', blank=True)
    email = models.EmailField(verbose_name='Email', blank=True)
    phone = models.CharField(max_length=20, verbose_name='Телефон', blank=True)
    address = models.TextField(verbose_name='Адрес', blank=True)
    notes = models.TextField(verbose_name='Заметки', blank=True)
    created_at = models.DateTimeField(auto_now_add=True, verbose_name='Создан')
    updated_at = models.DateTimeField(auto_now=True, verbose_name='Обновлен')
    
    class Meta:
        verbose_name = 'Поставщик'
        verbose_name_plural = 'Поставщики'
        ordering = ['name']
    
    def __str__(self):
        return self.name
    
    def get_absolute_url(self):
        return reverse('supplier_detail', kwargs={'pk': self.pk})


class Product(models.Model):
    """Товар"""
    name = models.CharField(max_length=200, verbose_name='Название товара')
    description = models.TextField(verbose_name='Описание', blank=True)
    category = models.ForeignKey(
        Category,
        on_delete=models.SET_NULL,
        null=True,
        blank=True,
        related_name='products',
        verbose_name='Категория'
    )
    supplier = models.ForeignKey(
        Supplier,
        on_delete=models.SET_NULL,
        null=True,
        blank=True,
        related_name='products',
        verbose_name='Поставщик'
    )
    
    # Цены
    purchase_price = models.DecimalField(
        max_digits=10,
        decimal_places=2,
        verbose_name='Закупочная цена',
        default=0,
        validators=[MinValueValidator(0)]
    )
    selling_price = models.DecimalField(
        max_digits=10,
        decimal_places=2,
        verbose_name='Цена продажи',
        default=0,
        validators=[MinValueValidator(0)]
    )
    
    # Склад
    stock_quantity = models.IntegerField(
        verbose_name='Количество на складе',
        default=0,
        validators=[MinValueValidator(0)]
    )
    min_stock_level = models.IntegerField(
        verbose_name='Минимальный остаток',
        default=0,
        validators=[MinValueValidator(0)],
        help_text='Уведомление при достижении этого уровня'
    )
    
    # Дополнительно
    sku = models.CharField(
        max_length=50,
        verbose_name='Артикул',
        blank=True,
        unique=True,
        null=True
    )
    barcode = models.CharField(
        max_length=50,
        verbose_name='Штрих-код',
        blank=True,
        unique=True,
        null=True
    )
    is_active = models.BooleanField(
        default=True,
        verbose_name='Активен',
        help_text='Отображается ли товар в продаже'
    )
    
    # Метаданные
    created_at = models.DateTimeField(auto_now_add=True, verbose_name='Создан')
    updated_at = models.DateTimeField(auto_now=True, verbose_name='Обновлен')
    created_by = models.ForeignKey(
        User,
        on_delete=models.SET_NULL,
        null=True,
        verbose_name='Создан пользователем'
    )
    
    class Meta:
        verbose_name = 'Товар'
        verbose_name_plural = 'Товары'
        ordering = ['name']
        indexes = [
            models.Index(fields=['sku']),
            models.Index(fields=['barcode']),
            models.Index(fields=['category']),
        ]
    
    def __str__(self):
        return self.name
    
    def get_absolute_url(self):
        return reverse('product_detail', kwargs={'pk': self.pk})
    
    def is_low_stock(self):
        """Проверить, низкий ли остаток"""
        return self.stock_quantity <= self.min_stock_level
    
    def get_profit_margin(self):
        """Рассчитать маржу"""
        if self.purchase_price > 0:
            return ((self.selling_price - self.purchase_price) / self.purchase_price) * 100
        return 0
    
    def get_total_value(self):
        """Общая стоимость товара на складе"""
        return self.stock_quantity * self.purchase_price


class Customer(models.Model):
    """Клиент магазина"""
    first_name = models.CharField(max_length=100, verbose_name='Имя')
    last_name = models.CharField(max_length=100, verbose_name='Фамилия', blank=True)
    email = models.EmailField(verbose_name='Email', blank=True)
    phone = models.CharField(max_length=20, verbose_name='Телефон', blank=True)
    address = models.TextField(verbose_name='Адрес', blank=True)
    notes = models.TextField(verbose_name='Заметки', blank=True)
    
    # Статистика
    total_orders = models.IntegerField(default=0, verbose_name='Всего заказов')
    total_spent = models.DecimalField(
        max_digits=10,
        decimal_places=2,
        default=0,
        verbose_name='Всего потрачено'
    )
    
    created_at = models.DateTimeField(auto_now_add=True, verbose_name='Создан')
    updated_at = models.DateTimeField(auto_now=True, verbose_name='Обновлен')
    
    class Meta:
        verbose_name = 'Клиент'
        verbose_name_plural = 'Клиенты'
        ordering = ['-created_at']
        indexes = [
            models.Index(fields=['email']),
            models.Index(fields=['phone']),
        ]
    
    def __str__(self):
        if self.last_name:
            return f"{self.first_name} {self.last_name}"
        return self.first_name
    
    def get_full_name(self):
        """Получить полное имя"""
        if self.last_name:
            return f"{self.first_name} {self.last_name}"
        return self.first_name
    
    def get_absolute_url(self):
        return reverse('customer_detail', kwargs={'pk': self.pk})
    
    def get_average_order_value(self):
        """Средний чек"""
        if self.total_orders > 0:
            return self.total_spent / self.total_orders
        return 0


class Order(models.Model):
    """Заказ/Продажа"""
    STATUS_CHOICES = [
        ('pending', 'Ожидает обработки'),
        ('processing', 'В обработке'),
        ('completed', 'Завершен'),
        ('cancelled', 'Отменен'),
    ]
    
    PAYMENT_STATUS_CHOICES = [
        ('unpaid', 'Не оплачено'),
        ('partial', 'Частично оплачено'),
        ('paid', 'Оплачено'),
        ('refunded', 'Возврат'),
    ]
    
    PAYMENT_METHOD_CHOICES = [
        ('cash', 'Наличные'),
        ('card', 'Карта'),
        ('online', 'Онлайн'),
        ('other', 'Другое'),
    ]
    
    # Связи
    customer = models.ForeignKey(
        Customer,
        on_delete=models.SET_NULL,
        null=True,
        blank=True,
        related_name='orders',
        verbose_name='Клиент'
    )
    
    # Информация о клиенте (если его нет в базе)
    customer_name = models.CharField(
        max_length=200,
        verbose_name='Имя клиента',
        blank=True
    )
    customer_phone = models.CharField(
        max_length=20,
        verbose_name='Телефон клиента',
        blank=True
    )
    customer_email = models.EmailField(
        verbose_name='Email клиента',
        blank=True
    )
    
    # Суммы
    subtotal = models.DecimalField(
        max_digits=10,
        decimal_places=2,
        verbose_name='Подытог',
        default=0
    )
    discount = models.DecimalField(
        max_digits=10,
        decimal_places=2,
        verbose_name='Скидка',
        default=0,
        validators=[MinValueValidator(0)]
    )
    total_amount = models.DecimalField(
        max_digits=10,
        decimal_places=2,
        verbose_name='Итого',
        default=0
    )
    
    # Статусы
    status = models.CharField(
        max_length=20,
        choices=STATUS_CHOICES,
        default='pending',
        verbose_name='Статус заказа'
    )
    payment_status = models.CharField(
        max_length=20,
        choices=PAYMENT_STATUS_CHOICES,
        default='unpaid',
        verbose_name='Статус оплаты'
    )
    payment_method = models.CharField(
        max_length=20,
        choices=PAYMENT_METHOD_CHOICES,
        default='cash',
        verbose_name='Способ оплаты'
    )
    
    # Дополнительно
    notes = models.TextField(verbose_name='Заметки', blank=True)
    
    # Метаданные
    created_at = models.DateTimeField(auto_now_add=True, verbose_name='Создан')
    updated_at = models.DateTimeField(auto_now=True, verbose_name='Обновлен')
    created_by = models.ForeignKey(
        User,
        on_delete=models.SET_NULL,
        null=True,
        verbose_name='Создан пользователем'
    )
    
    class Meta:
        verbose_name = 'Заказ'
        verbose_name_plural = 'Заказы'
        ordering = ['-created_at']
        indexes = [
            models.Index(fields=['status', 'created_at']),
            models.Index(fields=['customer', 'created_at']),
            models.Index(fields=['payment_status']),
        ]
    
    def __str__(self):
        return f"Заказ #{self.pk} - {self.created_at.strftime('%d.%m.%Y %H:%M')}"
    
    def get_absolute_url(self):
        return reverse('order_detail', kwargs={'pk': self.pk})
    
    def calculate_total(self):
        """Рассчитать итоговую сумму"""
        self.subtotal = sum(item.get_total() for item in self.items.all())
        # Не допускаем отрицательного итога из-за скидки
        self.total_amount = max(self.subtotal - self.discount, 0)
        self.save()
        return self.total_amount
    
    def get_customer_display_name(self):
        """Получить имя клиента для отображения"""
        if self.customer:
            return self.customer.get_full_name()
        return self.customer_name or 'Гость'
    
    def get_profit(self):
        """Рассчитать прибыль от заказа"""
        profit = 0
        for item in self.items.all():
            profit += (item.product.selling_price - item.product.purchase_price) * item.quantity
        return profit


class OrderItem(models.Model):
    """Позиция заказа"""
    order = models.ForeignKey(
        Order,
        on_delete=models.CASCADE,
        related_name='items',
        verbose_name='Заказ'
    )
    product = models.ForeignKey(
        Product,
        on_delete=models.PROTECT,
        related_name='order_items',
        verbose_name='Товар'
    )
    quantity = models.IntegerField(
        verbose_name='Количество',
        validators=[MinValueValidator(1)]
    )
    price = models.DecimalField(
        max_digits=10,
        decimal_places=2,
        verbose_name='Цена за единицу',
        help_text='Цена на момент продажи'
    )
    
    class Meta:
        verbose_name = 'Позиция заказа'
        verbose_name_plural = 'Позиции заказа'
        unique_together = ['order', 'product']
    
    def __str__(self):
        return f"{self.product.name} x{self.quantity}"
    
    def get_total(self):
        """Получить общую стоимость позиции"""
        return self.quantity * self.price
