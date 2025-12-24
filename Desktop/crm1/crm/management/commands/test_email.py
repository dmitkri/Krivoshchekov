"""
Management команда для отправки тестового email
Использование: python manage.py test_email [email@example.com]
"""
from django.core.management.base import BaseCommand
from django.core.mail import send_mail
from django.conf import settings


class Command(BaseCommand):
    help = 'Отправить тестовое письмо для проверки настроек email'

    def add_arguments(self, parser):
        parser.add_argument(
            'email',
            nargs='?',
            type=str,
            default='storehouse.vlog@gmail.com',
            help='Email адрес получателя (по умолчанию: storehouse.vlog@gmail.com)',
        )

    def handle(self, *args, **options):
        recipient_email = options['email']
        
        self.stdout.write('=' * 50)
        self.stdout.write('Настройки email:')
        self.stdout.write(f'  EMAIL_BACKEND: {settings.EMAIL_BACKEND}')
        self.stdout.write(f'  EMAIL_HOST: {settings.EMAIL_HOST}')
        self.stdout.write(f'  EMAIL_PORT: {settings.EMAIL_PORT}')
        self.stdout.write(f'  EMAIL_USE_TLS: {settings.EMAIL_USE_TLS}')
        self.stdout.write(f'  EMAIL_HOST_USER: {settings.EMAIL_HOST_USER}')
        self.stdout.write(f'  DEFAULT_FROM_EMAIL: {settings.DEFAULT_FROM_EMAIL}')
        self.stdout.write('=' * 50)
        self.stdout.write('')
        
        if not settings.EMAIL_HOST_USER or not hasattr(settings, 'EMAIL_HOST_PASSWORD') or not settings.EMAIL_HOST_PASSWORD:
            self.stdout.write(
                self.style.WARNING(
                    '⚠️  EMAIL_HOST_USER или EMAIL_HOST_PASSWORD не настроены!'
                )
            )
            self.stdout.write('Проверьте файл .env и убедитесь, что заполнены все настройки.')
            return
        
        self.stdout.write(f'Отправка тестового письма на {recipient_email}...')
        
        try:
            result = send_mail(
                subject='Тестовое сообщение из CRM',
                message='''Это тестовое письмо из CRM системы.

Если вы получили это письмо, значит настройка почты работает правильно!

Система будет автоматически отправлять уведомления клиентам об изменении статуса их заказов.
''',
                from_email=settings.DEFAULT_FROM_EMAIL,
                recipient_list=[recipient_email],
                fail_silently=False,
            )
            
            if result:
                self.stdout.write(
                    self.style.SUCCESS(
                        f'✅ Письмо успешно отправлено на {recipient_email}!'
                    )
                )
            else:
                self.stdout.write(
                    self.style.ERROR('❌ Не удалось отправить письмо')
                )
                
        except Exception as e:
            self.stdout.write(
                self.style.ERROR(f'❌ Ошибка при отправке письма: {e}')
            )
            self.stdout.write('')
            self.stdout.write('Возможные причины:')
            self.stdout.write('1. Неверный пароль приложения Gmail')
            self.stdout.write('2. Проблемы с SSL сертификатами (на macOS)')
            self.stdout.write('3. Неверные настройки SMTP')
            self.stdout.write('')
            self.stdout.write('Проверьте файл .env и убедитесь, что:')
            self.stdout.write('- EMAIL_HOST_PASSWORD содержит пароль приложения (App Password)')
            self.stdout.write('- Для Gmail включена двухфакторная аутентификация')
            self.stdout.write('- Создан пароль приложения: https://myaccount.google.com/apppasswords')

