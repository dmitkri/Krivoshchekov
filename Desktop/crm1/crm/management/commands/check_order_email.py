"""
Команда для проверки отправки email для конкретного заказа
"""
from django.core.management.base import BaseCommand
from crm.models import Order
from crm.views import send_order_status_email


class Command(BaseCommand):
    help = 'Проверить и отправить email для заказа'

    def add_arguments(self, parser):
        parser.add_argument('order_id', type=int, help='ID заказа')

    def handle(self, *args, **options):
        order_id = options['order_id']
        
        try:
            order = Order.objects.get(pk=order_id)
        except Order.DoesNotExist:
            self.stdout.write(self.style.ERROR(f'Заказ #{order_id} не найден'))
            return
        
        self.stdout.write('=' * 60)
        self.stdout.write(f'Информация о заказе #{order.id}')
        self.stdout.write('=' * 60)
        self.stdout.write(f'Статус: {order.status}')
        self.stdout.write(f'Email в заказе: {order.customer_email or "не указан"}')
        if order.customer:
            self.stdout.write(f'Клиент: {order.customer.get_full_name()}')
            self.stdout.write(f'Email клиента: {order.customer.email or "не указан"}')
        else:
            self.stdout.write('Клиент: не указан')
        
        # Определяем email
        customer_email = None
        if order.customer_email:
            customer_email = order.customer_email
            self.stdout.write(f'\n✅ Email найден в заказе: {customer_email}')
        elif order.customer and order.customer.email:
            customer_email = order.customer.email
            self.stdout.write(f'\n✅ Email найден у клиента: {customer_email}')
        else:
            self.stdout.write(self.style.WARNING('\n⚠️  Email не найден!'))
            return
        
        # Проверяем отправку
        self.stdout.write('\n' + '=' * 60)
        self.stdout.write('Тестирование отправки email...')
        self.stdout.write('=' * 60)
        
        old_status = order.status
        new_status = 'completed' if old_status != 'completed' else 'processing'
        
        self.stdout.write(f'Старый статус: {old_status}')
        self.stdout.write(f'Новый статус (тестовый): {new_status}')
        
        try:
            result = send_order_status_email(order, old_status, new_status)
            if result:
                self.stdout.write(self.style.SUCCESS('\n✅ Письмо успешно отправлено!'))
            else:
                self.stdout.write(self.style.ERROR('\n❌ Не удалось отправить письмо'))
        except Exception as e:
            self.stdout.write(self.style.ERROR(f'\n❌ Ошибка: {e}'))
            import traceback
            self.stdout.write(traceback.format_exc())

