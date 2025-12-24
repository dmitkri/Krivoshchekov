"""
Management команда для отправки писем всем, у кого есть email в базе
Использование: python manage.py send_emails_to_all
"""
from django.core.management.base import BaseCommand
from django.core.mail import send_mail
from django.conf import settings
from django.db import models
from crm.models import Customer, Order


class Command(BaseCommand):
    help = 'Отправить тестовое письмо всем, у кого есть email в базе данных'

    def add_arguments(self, parser):
        parser.add_argument(
            '--test',
            action='store_true',
            help='Отправить тестовое письмо (по умолчанию)',
        )
        parser.add_argument(
            '--orders',
            action='store_true',
            help='Отправить информацию о заказах (если есть)',
        )

    def handle(self, *args, **options):
        self.stdout.write('=' * 60)
        self.stdout.write('Поиск всех email адресов в базе данных...')
        self.stdout.write('=' * 60)
        
        # Собираем все уникальные email адреса
        emails_set = set()
        email_info = {}  # email -> информация о владельце
        
        # Проверяем клиентов
        customers_with_email = Customer.objects.exclude(email='').exclude(email__isnull=True).filter(email__gt='')
        self.stdout.write(f'\nКлиенты с email: {customers_with_email.count()}')
        
        for customer in customers_with_email:
            if customer.email:
                email = customer.email.strip().lower()
                emails_set.add(email)
                if email not in email_info:
                    email_info[email] = []
                email_info[email].append({
                    'type': 'Клиент',
                    'name': customer.get_full_name(),
                    'id': customer.id,
                    'orders_count': customer.orders.count()
                })
                self.stdout.write(f'  - {customer.get_full_name()} ({customer.email})')
        
        # Проверяем заказы с email
        orders_with_email = Order.objects.exclude(customer_email='').exclude(customer_email__isnull=True).filter(customer_email__gt='')
        self.stdout.write(f'\nЗаказы с email: {orders_with_email.count()}')
        
        for order in orders_with_email:
            if order.customer_email:
                email = order.customer_email.strip().lower()
                emails_set.add(email)
                if email not in email_info:
                    email_info[email] = []
                email_info[email].append({
                    'type': 'Заказ',
                    'name': order.customer_name or 'Без имени',
                    'id': order.id,
                    'order_number': order.id
                })
                self.stdout.write(f'  - Заказ #{order.id} ({order.customer_email}) - {order.customer_name or "Без имени"}')
        
        total_unique_emails = len(emails_set)
        
        self.stdout.write('\n' + '=' * 60)
        self.stdout.write(f'Всего найдено уникальных email адресов: {total_unique_emails}')
        self.stdout.write('=' * 60)
        
        if total_unique_emails == 0:
            self.stdout.write(self.style.WARNING('\n⚠️  Не найдено ни одного email адреса в базе данных!'))
            return
        
        # Подтверждение отправки
        self.stdout.write('\nСписок email адресов для отправки:')
        for i, email in enumerate(sorted(emails_set), 1):
            info_list = email_info.get(email, [])
            info_str = ', '.join([f"{item['type']} #{item['id']}" for item in info_list])
            self.stdout.write(f'  {i}. {email} ({info_str})')
        
        self.stdout.write('\n' + '=' * 60)
        self.stdout.write('Отправка писем...')
        self.stdout.write('=' * 60)
        
        success_count = 0
        error_count = 0
        
        for email in sorted(emails_set):
            try:
                # Формируем содержимое письма
                info_list = email_info.get(email, [])
                
                # Собираем информацию о заказах этого email
                customer_orders = Order.objects.filter(
                    models.Q(customer_email__iexact=email) |
                    models.Q(customer__email__iexact=email)
                ).order_by('-created_at')[:10]
                
                if options.get('orders') and customer_orders.exists():
                    # Письмо с информацией о заказах
                    orders_info = []
                    for order in customer_orders:
                        status_choices = dict(Order.STATUS_CHOICES)
                        status_display = status_choices.get(order.status, order.status)
                        orders_info.append(f"Заказ #{order.id} от {order.created_at.strftime('%d.%m.%Y')} - {status_display} - {order.total_amount} руб.")
                    
                    message = f'''Здравствуйте!

Вы получаете это письмо, потому что в нашей базе данных есть ваши заказы.

Ваши последние заказы:
{chr(10).join(orders_info)}

С уважением,
CRM Система'''
                    
                    subject = 'Информация о ваших заказах'
                else:
                    # Тестовое письмо
                    message = f'''Здравствуйте!

Это тестовое письмо из CRM системы.

Ваш email найден в нашей базе данных.
Система будет автоматически отправлять вам уведомления об изменении статуса ваших заказов.

С уважением,
CRM Система'''
                    
                    subject = 'Тестовое сообщение из CRM системы'
                
                # Отправляем письмо
                send_mail(
                    subject=subject,
                    message=message,
                    from_email=settings.DEFAULT_FROM_EMAIL,
                    recipient_list=[email],
                    fail_silently=True,
                )
                
                self.stdout.write(self.style.SUCCESS(f'  ✅ Отправлено на {email}'))
                success_count += 1
                
            except Exception as e:
                self.stdout.write(self.style.ERROR(f'  ❌ Ошибка для {email}: {e}'))
                error_count += 1
        
        self.stdout.write('\n' + '=' * 60)
        self.stdout.write(f'Итого отправлено: {success_count}')
        if error_count > 0:
            self.stdout.write(self.style.ERROR(f'Ошибок: {error_count}'))
        self.stdout.write('=' * 60)

