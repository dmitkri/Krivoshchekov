"""
Management команда для тестирования отправки email об изменении статуса заказа
Использование: python manage.py test_order_email <order_id> <new_status>
"""
from django.core.management.base import BaseCommand
from django.core.exceptions import ObjectDoesNotExist
from crm.models import Order
from crm.views import send_order_status_email


class Command(BaseCommand):
    help = 'Тестирование отправки email об изменении статуса заказа'

    def add_arguments(self, parser):
        parser.add_argument(
            'order_id',
            type=int,
            help='ID заказа для тестирования',
        )
        parser.add_argument(
            'new_status',
            type=str,
            help='Новый статус для тестирования (pending, processing, completed, cancelled)',
        )

    def handle(self, *args, **options):
        order_id = options['order_id']
        new_status = options['new_status']
        
        # Проверяем, что статус валидный
        valid_statuses = [choice[0] for choice in Order.STATUS_CHOICES]
        if new_status not in valid_statuses:
            self.stdout.write(
                self.style.ERROR(
                    f'Неверный статус: {new_status}. '
                    f'Допустимые значения: {", ".join(valid_statuses)}'
                )
            )
            return
        
        try:
            order = Order.objects.get(pk=order_id)
        except ObjectDoesNotExist:
            self.stdout.write(
                self.style.ERROR(f'Заказ с ID {order_id} не найден')
            )
            return
        
        self.stdout.write('=' * 50)
        self.stdout.write(f'Заказ #{order.id}')
        self.stdout.write(f'  Клиент: {order.get_customer_display_name()}')
        self.stdout.write(f'  Текущий статус: {order.status}')
        self.stdout.write(f'  Новый статус: {new_status}')
        
        # Определяем email
        customer_email = None
        if order.customer_email:
            customer_email = order.customer_email
            self.stdout.write(f'  Email: {customer_email} (из заказа)')
        elif order.customer and order.customer.email:
            customer_email = order.customer.email
            self.stdout.write(f'  Email: {customer_email} (из клиента)')
        else:
            self.stdout.write(
                self.style.WARNING('  ⚠️  Email не указан ни в заказе, ни у клиента!')
            )
        self.stdout.write('=' * 50)
        
        if not customer_email:
            self.stdout.write(
                self.style.ERROR(
                    '❌ Невозможно отправить письмо: email не указан'
                )
            )
            self.stdout.write('')
            self.stdout.write('Укажите email в заказе или у связанного клиента.')
            return
        
        old_status = order.status
        self.stdout.write('')
        self.stdout.write(f'Отправка тестового письма на {customer_email}...')
        
        try:
            result = send_order_status_email(order, old_status, new_status)
            
            if result:
                self.stdout.write(
                    self.style.SUCCESS(
                        f'✅ Письмо успешно отправлено на {customer_email}!'
                    )
                )
                self.stdout.write('')
                self.stdout.write('Проверьте почту - должно прийти письмо об изменении статуса заказа.')
            else:
                self.stdout.write(
                    self.style.ERROR('❌ Не удалось отправить письмо')
                )
        except Exception as e:
            self.stdout.write(
                self.style.ERROR(f'❌ Ошибка при отправке письма: {e}')
            )

