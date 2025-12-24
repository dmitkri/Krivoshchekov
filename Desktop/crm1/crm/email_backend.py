"""
Кастомный email backend для исправления проблем с SSL на macOS
"""
import ssl
from django.core.mail.backends.smtp import EmailBackend
import warnings


class FixedEmailBackend(EmailBackend):
    """
    Email backend с исправлением проблем SSL для development окружения.
    
    Отключает проверку SSL сертификатов для упрощения настройки email
    в development. НЕ ИСПОЛЬЗУЙТЕ в production!
    """
    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)
        warnings.warn(
            "Используется небезопасный SSL контекст для email. "
            "Это только для development!",
            UserWarning
        )
        if self.use_tls or self.use_ssl:
            self.ssl_context = ssl.create_default_context()
            self.ssl_context.check_hostname = False
            self.ssl_context.verify_mode = ssl.CERT_NONE

