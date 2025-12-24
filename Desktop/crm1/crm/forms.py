"""
Формы для CRM системы магазина
"""
from django import forms
from django.core.validators import MinValueValidator
from .models import (
    Category, Supplier, Product, Customer, Order, OrderItem
)


class CategoryForm(forms.ModelForm):
    """Форма для категории"""
    class Meta:
        model = Category
        fields = ['name', 'description']
        widgets = {
            'name': forms.TextInput(attrs={'class': 'form-control', 'required': True}),
            'description': forms.Textarea(attrs={'class': 'form-control', 'rows': 3}),
        }


class SupplierForm(forms.ModelForm):
    """Форма для поставщика"""
    class Meta:
        model = Supplier
        fields = ['name', 'contact_person', 'email', 'phone', 'address', 'notes']
        widgets = {
            'name': forms.TextInput(attrs={'class': 'form-control', 'required': True}),
            'contact_person': forms.TextInput(attrs={'class': 'form-control'}),
            'email': forms.EmailInput(attrs={'class': 'form-control'}),
            'phone': forms.TextInput(attrs={'class': 'form-control'}),
            'address': forms.Textarea(attrs={'class': 'form-control', 'rows': 3}),
            'notes': forms.Textarea(attrs={'class': 'form-control', 'rows': 3}),
        }


class ProductForm(forms.ModelForm):
    """Форма для товара"""
    class Meta:
        model = Product
        fields = [
            'name', 'description', 'category', 'supplier',
            'purchase_price', 'selling_price',
            'stock_quantity', 'min_stock_level',
            'sku', 'barcode', 'is_active'
        ]
        widgets = {
            'name': forms.TextInput(attrs={'class': 'form-control', 'required': True}),
            'description': forms.Textarea(attrs={'class': 'form-control', 'rows': 4}),
            'category': forms.Select(attrs={'class': 'form-control'}),
            'supplier': forms.Select(attrs={'class': 'form-control'}),
            'purchase_price': forms.NumberInput(attrs={
                'class': 'form-control',
                'step': '0.01',
                'min': '0'
            }),
            'selling_price': forms.NumberInput(attrs={
                'class': 'form-control',
                'step': '0.01',
                'min': '0'
            }),
            'stock_quantity': forms.NumberInput(attrs={
                'class': 'form-control',
                'min': '0'
            }),
            'min_stock_level': forms.NumberInput(attrs={
                'class': 'form-control',
                'min': '0'
            }),
            'sku': forms.TextInput(attrs={'class': 'form-control'}),
            'barcode': forms.TextInput(attrs={'class': 'form-control'}),
            'is_active': forms.CheckboxInput(attrs={'class': 'form-check-input'}),
        }
    
    def clean_selling_price(self):
        selling_price = self.cleaned_data.get('selling_price')
        purchase_price = self.cleaned_data.get('purchase_price')
        if selling_price and purchase_price and selling_price < purchase_price:
            raise forms.ValidationError(
                'Цена продажи не должна быть меньше закупочной цены'
            )
        return selling_price


class CustomerForm(forms.ModelForm):
    """Форма для клиента"""
    class Meta:
        model = Customer
        fields = ['first_name', 'last_name', 'email', 'phone', 'address', 'notes']
        widgets = {
            'first_name': forms.TextInput(attrs={'class': 'form-control', 'required': True}),
            'last_name': forms.TextInput(attrs={'class': 'form-control'}),
            'email': forms.EmailInput(attrs={'class': 'form-control'}),
            'phone': forms.TextInput(attrs={'class': 'form-control'}),
            'address': forms.Textarea(attrs={'class': 'form-control', 'rows': 3}),
            'notes': forms.Textarea(attrs={'class': 'form-control', 'rows': 3}),
        }
    
    def clean_first_name(self):
        first_name = self.cleaned_data.get('first_name')
        if not first_name or len(first_name.strip()) == 0:
            raise forms.ValidationError('Имя обязательно для заполнения')
        return first_name.strip()


class OrderForm(forms.ModelForm):
    """Форма для заказа"""
    class Meta:
        model = Order
        fields = [
            'customer', 'customer_name', 'customer_phone', 'customer_email',
            'status', 'payment_status', 'payment_method',
            'discount', 'notes'
        ]
        widgets = {
            'customer': forms.Select(attrs={'class': 'form-control'}),
            'customer_name': forms.TextInput(attrs={'class': 'form-control'}),
            'customer_phone': forms.TextInput(attrs={'class': 'form-control'}),
            'customer_email': forms.EmailInput(attrs={'class': 'form-control'}),
            'status': forms.Select(attrs={'class': 'form-control'}),
            'payment_status': forms.Select(attrs={'class': 'form-control'}),
            'payment_method': forms.Select(attrs={'class': 'form-control'}),
            'discount': forms.NumberInput(attrs={
                'class': 'form-control',
                'step': '0.01',
                'min': '0'
            }),
            'notes': forms.Textarea(attrs={'class': 'form-control', 'rows': 3}),
        }
    
    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)
        self.fields['customer'].queryset = Customer.objects.all().order_by('first_name', 'last_name')
        self.fields['customer'].required = False
    
    def clean(self):
        cleaned_data = super().clean()
        status = cleaned_data.get('status')
        payment_status = cleaned_data.get('payment_status')
        
        # Проверка: нельзя завершить заказ, если он не оплачен
        if status == 'completed' and payment_status != 'paid':
            payment_status_choices = dict(self.Meta.model.PAYMENT_STATUS_CHOICES)
            payment_status_display = payment_status_choices.get(payment_status, payment_status)
            raise forms.ValidationError(
                f'Нельзя завершить заказ, если он не оплачен. '
                f'Текущий статус оплаты: "{payment_status_display}". '
                f'Пожалуйста, сначала измените статус оплаты на "Оплачено" или выберите другой статус заказа.'
            )
        
        return cleaned_data


class OrderItemForm(forms.ModelForm):
    """Форма для позиции заказа"""
    class Meta:
        model = OrderItem
        fields = ['product', 'quantity', 'price']
        widgets = {
            'product': forms.Select(attrs={'class': 'form-control'}),
            'quantity': forms.NumberInput(attrs={
                'class': 'form-control',
                'min': '1'
            }),
            'price': forms.NumberInput(attrs={
                'class': 'form-control',
                'step': '0.01',
                'min': '0',
                'placeholder': 'Автоматически из товара'
            }),
        }
    
    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)
        self.fields['product'].queryset = Product.objects.filter(is_active=True).order_by('name')
        # Делаем поле цены необязательным, так как оно будет заполняться автоматически
        self.fields['price'].required = False
    
    def clean_quantity(self):
        quantity = self.cleaned_data.get('quantity')
        product = self.cleaned_data.get('product')
        
        if quantity and product:
            # Разрешаем использовать уже зарезервированное количество в этом заказе
            existing_reserved = 0
            if self.instance and self.instance.pk and self.instance.product_id == product.id:
                existing_reserved = self.instance.quantity or 0
            
            available = product.stock_quantity + existing_reserved
            if quantity > available:
                raise forms.ValidationError(
                    f'Недостаточно товара на складе. Доступно: {available}'
                )
        
        return quantity
    
    def clean(self):
        cleaned_data = super().clean()
        product = cleaned_data.get('product')
        price = cleaned_data.get('price')
        
        # Обрабатываем разные случаи пустых значений
        price_is_empty = (
            price is None or 
            price == '' or 
            price == 0 or 
            (isinstance(price, (int, float)) and price == 0)
        )
        
        # Если цена не указана или равна 0, использовать цену товара
        if product and price_is_empty:
            if product.selling_price and product.selling_price > 0:
                cleaned_data['price'] = product.selling_price
            else:
                raise forms.ValidationError(
                    f'У товара "{product.name}" не указана цена продажи. Пожалуйста, укажите цену вручную или установите цену товара.'
                )
        elif product and price and not price_is_empty:
            # Проверка, что цена больше 0
            if price <= 0:
                raise forms.ValidationError('Цена должна быть больше нуля')
        
        return cleaned_data
    
    def save(self, commit=True):
        """Сохранить позицию заказа с автоматической установкой цены из товара"""
        instance = super().save(commit=False)
        product = instance.product
        
        # Если цена не указана или равна 0, использовать цену товара
        if product and (not instance.price or instance.price == 0):
            if product.selling_price and product.selling_price > 0:
                instance.price = product.selling_price
            else:
                # Если у товара нет цены, использовать значение из формы или 0
                if hasattr(self, 'cleaned_data') and self.cleaned_data.get('price'):
                    instance.price = self.cleaned_data['price']
        
        if commit:
            instance.save()
        return instance


OrderItemFormSet = forms.inlineformset_factory(
    Order,
    OrderItem,
    form=OrderItemForm,
    extra=0,  # не добавляем пустую строку по умолчанию
    can_delete=True,
    min_num=1,
    validate_min=True
)
