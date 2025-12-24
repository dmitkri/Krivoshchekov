"""
Настройка административной панели Django для CRM системы
"""
from django.contrib import admin
from .models import Category, Supplier, Product, Customer, Order, OrderItem


@admin.register(Category)
class CategoryAdmin(admin.ModelAdmin):
    """Административная панель для управления категориями товаров"""
    list_display = ['name', 'created_at']
    list_filter = ['created_at']
    # Поиск регистронезависимый (Django Admin использует icontains по умолчанию)
    search_fields = ['name']


@admin.register(Supplier)
class SupplierAdmin(admin.ModelAdmin):
    """Административная панель для управления поставщиками"""
    list_display = ['name', 'contact_person', 'phone', 'email']
    # Поиск регистронезависимый (Django Admin использует icontains по умолчанию)
    search_fields = ['name', 'contact_person', 'phone', 'email']


@admin.register(Product)
class ProductAdmin(admin.ModelAdmin):
    """Административная панель для управления товарами"""
    list_display = ['name', 'category', 'selling_price', 'stock_quantity', 'is_active']
    list_filter = ['category', 'is_active', 'supplier']
    # Поиск регистронезависимый (Django Admin использует icontains по умолчанию)
    search_fields = ['name', 'sku', 'barcode']
    readonly_fields = ['created_at', 'updated_at']


@admin.register(Customer)
class CustomerAdmin(admin.ModelAdmin):
    """Административная панель для управления клиентами"""
    list_display = ['first_name', 'last_name', 'email', 'phone', 'total_orders', 'total_spent']
    # Поиск регистронезависимый (Django Admin использует icontains по умолчанию)
    search_fields = ['first_name', 'last_name', 'email', 'phone']
    readonly_fields = ['total_orders', 'total_spent', 'created_at', 'updated_at']


class OrderItemInline(admin.TabularInline):
    """Инлайн-форма для управления позициями заказа в административной панели"""
    model = OrderItem
    extra = 1
    readonly_fields = ['get_total']


@admin.register(Order)
class OrderAdmin(admin.ModelAdmin):
    """Административная панель для управления заказами"""
    list_display = ['id', 'get_customer_display_name', 'total_amount', 'status', 'payment_status', 'created_at']
    list_filter = ['status', 'payment_status', 'payment_method', 'created_at']
    # Поиск регистронезависимый (Django Admin использует icontains по умолчанию)
    search_fields = ['customer__first_name', 'customer__last_name', 'customer_name']
    readonly_fields = ['created_at', 'updated_at']
    inlines = [OrderItemInline]
    
    def get_customer_display_name(self, obj):
        """Получить отображаемое имя клиента для списка заказов"""
        return obj.get_customer_display_name()
    get_customer_display_name.short_description = 'Клиент'
