# Tripwithfriends API

Спешиал фор расчет кто и сколько кому должен

## Технологический стек

- **ASP.NET Core 10.0** (Web API)
- **PostgreSQL** (база данных)
- **Redis** (кэширование)
- **Liquibase** (миграции базы данных)
- **Entity Framework Core** (ORM)
- **Dapper** (для сложных запросов с транзакциями)
- **JWT** (аутентификация пользователей)
- **FluentValidation** (валидация)
- **Swagger** (документация API)
- **Prometheus** (метрики)
- **Docker** (контейнеризация)

## Быстрый старт

### 1. Запуск через Docker Compose

```bash
# Клонируйте репозиторий
git clone <repository-url>
cd Tripwithfriends

# Запустите все сервисы (API, PostgreSQL, Redis, Liquibase)
docker-compose up -d --build

# Проверьте статус
docker-compose ps

# Просмотрите логи
docker-compose logs -f api
```

После запуска:
- **API**: http://localhost:5001
- **Swagger UI**: http://localhost:5001/swagger
- **Health Check**: http://localhost:5001/health
- **Prometheus Metrics**: http://localhost:5001/metrics

### 2. Остановка сервисов

```bash
docker-compose down

# С удалением данных
docker-compose down -v
```

## Пошаговая инструкция по использованию

### Шаг 1: Регистрация пользователя

Создайте первого пользователя (обычно это будет Admin):

```bash
curl -X POST http://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Николай Соболев",
    "email": "sobolev@youtube.com",
    "password": "TopTopTop2024!",
    "role": "Admin"
  }'
```

**Ответ:**
```json
{
  "id": "7c7cc42a-d382-49a3-b407-2e211bab5b53",
  "name": "Николай Соболев",
  "email": "sobolev@youtube.com",
  "role": "Admin"
}
```

**Роли:**
- `Admin` - полный доступ
- `Manager` - чтение, создание, редактирование
- `User` - чтение и добавление расходов

### Шаг 2: Вход в систему (получение JWT токена)

```bash
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "sobolev@youtube.com",
    "password": "TopTopTop2024!"
  }'
```

**Ответ:**
```json
{
  "jwtToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "USER_ID",
    "name": "Николай Соболев",
    "email": "sobolev@youtube.com",
    "role": "Admin"
  }
}
```

**Важно:**
- **JWT токен** (`jwtToken`) - используется в заголовке `Authorization: Bearer <token>` для аутентификации
- **Сохраните токен** - он понадобится для всех защищенных запросов!

### Шаг 3: Создание поездки

Сначала создайте еще несколько пользователей для участия в поездке:

```bash
# Создаем пользователя 1
curl -X POST http://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Игорь Синяк",
    "email": "sinyak@instagram.com",
    "password": "CrazyRussian2024!",
    "role": "User"
  }'

# Создаем пользователя 2
curl -X POST http://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Мария Вэй",
    "email": "wei@youtube.com",
    "password": "BeautyBlogger2024!",
    "role": "User"
  }'
```

Теперь создайте поездку (используйте ID пользователей из ответов регистрации):

```bash
curl -X POST http://localhost:5001/api/trips \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "name": "Поездка в Сочи",
    "startDate": "2026-07-01T00:00:00Z",
    "endDate": "2026-07-10T00:00:00Z",
    "participantIds": [
      "7c7cc42a-d382-49a3-b407-2e211bab5b53",
      "USER1_ID",
      "USER2_ID"
    ]
  }'
```

**Ответ:**
```json
{
  "id": "f357628a-259f-45a5-8d2f-35af3081959c",
  "name": "Поездка в Сочи",
  "startDate": "2026-07-01T00:00:00Z",
  "endDate": "2026-07-10T00:00:00Z",
  "organizerId": "7c7cc42a-d382-49a3-b407-2e211bab5b53",
  "organizerName": "Николай Соболев",
  "isCompleted": false,
  "participantIds": [
    "7c7cc42a-d382-49a3-b407-2e211bab5b53",
    "USER1_ID",
    "USER2_ID"
  ]
}
```

**Сохраните ID поездки** - он понадобится для работы с расходами.

### Шаг 4: Добавление расходов

Добавьте расходы в поездку:

```bash
curl -X POST http://localhost:5001/api/trips/TRIP_ID/expenses \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "name": "Такси из аэропорта",
    "amount": 1500.00,
    "category": "Транспорт",
    "date": "2026-07-01T10:00:00Z",
    "payerId": "USER1_ID",
    "participantIds": [
      "USER1_ID",
      "USER2_ID"
    ]
  }'
```

**Важно:** Сумма расхода автоматически делится поровну между всеми участниками из `participantIds`.

Добавьте еще несколько расходов:

```bash
# Обед в ресторане
curl -X POST http://localhost:5001/api/trips/TRIP_ID/expenses \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "name": "Обед в ресторане",
    "amount": 3000.00,
    "category": "Еда",
    "date": "2026-07-01T14:00:00Z",
    "payerId": "USER2_ID",
    "participantIds": ["USER1_ID", "USER2_ID"]
  }'

# Отель
curl -X POST http://localhost:5001/api/trips/TRIP_ID/expenses \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "name": "Отель на 3 ночи",
    "amount": 12000.00,
    "category": "Проживание",
    "date": "2026-07-01T15:00:00Z",
    "payerId": "USER1_ID",
    "participantIds": ["USER1_ID", "USER2_ID"]
  }'
```

### Шаг 5: Просмотр балансов

Получите баланс всех участников поездки:

```bash
curl -X GET http://localhost:5001/api/trips/TRIP_ID/balance \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**Ответ:**
```json
[
  {
    "userId": "USER1_ID",
    "userName": "Игорь Синяк",
    "paidAmount": 13500.00,
    "owedAmount": 8250.00,
    "balance": 5250.00
  },
  {
    "userId": "USER2_ID",
    "userName": "Мария Вэй",
    "paidAmount": 3000.00,
    "owedAmount": 8250.00,
    "balance": -5250.00
  }
]
```

**Интерпретация:**
- **Положительный баланс** (5250.00) - пользователю должны деньги
- **Отрицательный баланс** (-5250.00) - пользователь должен деньги

В данном примере: Мария Вэй должна Игорю Синяку 5250 рублей.

### Шаг 6: Просмотр расходов с фильтрацией и пагинацией

```bash
# Получить все расходы (первая страница, 10 записей)
curl -X GET "http://localhost:5001/api/trips/TRIP_ID/expenses?page=1&pageSize=10" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"

# Фильтрация по категории
curl -X GET "http://localhost:5001/api/trips/TRIP_ID/expenses?category=Транспорт" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"

# Поиск по названию
curl -X GET "http://localhost:5001/api/trips/TRIP_ID/expenses?search=Такси" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"

# Комбинированная фильтрация
curl -X GET "http://localhost:5001/api/trips/TRIP_ID/expenses?page=1&pageSize=5&category=Еда&search=Обед" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**Ответ:**
```json
{
  "items": [
    {
      "id": "...",
      "name": "Обед в ресторане",
      "amount": 3000.00,
      "category": "Еда",
      "date": "2026-07-01T14:00:00Z",
      "payerId": "USER2_ID",
      "payerName": "Мария Вэй",
      "tripId": "TRIP_ID",
      "participantIds": ["USER1_ID", "USER2_ID"]
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 5,
  "totalPages": 1
}
```

### Шаг 7: Завершение поездки

После того, как все расходы добавлены, завершите поездку:

```bash
curl -X POST http://localhost:5001/api/trips/TRIP_ID/complete \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**Важно:** После завершения поездки нельзя добавлять или изменять расходы.


## Idempotency

Для POST запросов можно использовать заголовок `Idempotency-Key`:

```bash
curl -X POST http://localhost:5001/api/trips \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Idempotency-Key: unique-request-id-12345" \
  -d '{
    "name": "Поездка в Сочи",
    "startDate": "2026-07-01T00:00:00Z",
    "endDate": "2026-07-10T00:00:00Z",
    "participantIds": []
  }'
```

Если запрос с таким же `Idempotency-Key` будет отправлен повторно, вернется тот же результат без создания дубликата.

## Мониторинг и метрики

### Health Check

```bash
curl http://localhost:5001/health
```

**Ответ:**
```json
{
  "status": "Healthy",
  "checks": {
    "postgresql": "Healthy",
    "redis": "Healthy"
  }
}
```

### Prometheus Metrics

Метрики доступны по адресу: http://localhost:5001/metrics

Основные метрики:
- `http_requests_total` - общее количество HTTP запросов
- `http_request_duration_seconds` - время выполнения запросов
- `http_requests_in_flight` - количество активных запросов

## Swagger UI

Интерактивная документация API доступна по адресу: http://localhost:5001/swagger

В Swagger UI можно:
1. Просмотреть все эндпоинты
2. Протестировать API прямо в браузере
3. Авторизоваться через JWT токен
4. Увидеть примеры запросов и ответов

## Полный список эндпоинтов

### Аутентификация
- `POST /api/auth/register` - Регистрация пользователя
- `POST /api/auth/login` - Вход (получение JWT токена)

### Пользователи
- `GET /api/users` - Список пользователей (Admin/Manager)
- `GET /api/users/{id}` - Получить пользователя
- `POST /api/users` - Создать пользователя (Admin)
- `PUT /api/users/{id}` - Обновить пользователя (Admin)
- `DELETE /api/users/{id}` - Удалить пользователя (Admin)

### Поездки
- `GET /api/trips` - Список всех поездок
- `GET /api/trips/{id}` - Получить поездку (кэшируется 5 минут)
- `POST /api/trips` - Создать поездку
- `PUT /api/trips/{id}` - Обновить поездку (организатор/Admin/Manager)
- `DELETE /api/trips/{id}` - Удалить поездку (организатор/Admin)
- `POST /api/trips/{id}/complete` - Завершить поездку (организатор/Admin/Manager)
- `GET /api/trips/{id}/balance` - Получить баланс участников

### Расходы
- `GET /api/trips/{tripId}/expenses` - Список расходов (с пагинацией и фильтрацией)
- `GET /api/trips/{tripId}/expenses/{id}` - Получить расход
- `POST /api/trips/{tripId}/expenses` - Создать расход
- `PUT /api/trips/{tripId}/expenses/{id}` - Обновить расход (Admin/Manager)
- `DELETE /api/trips/{tripId}/expenses/{id}` - Удалить расход (Admin/Manager)

## Роли и права доступа

### Admin
- ✅ Полный доступ ко всем операциям
- ✅ Управление пользователями
- ✅ Удаление любых поездок и расходов

### Manager
- ✅ Чтение всех данных
- ✅ Создание и редактирование поездок и расходов
- ❌ Не может удалять поездки и расходы
- ❌ Не может управлять пользователями

### User
- ✅ Чтение поездок и расходов
- ✅ Создание поездок
- ✅ Добавление расходов
- ❌ Не может редактировать или удалять существующие данные

## Логика расчёта балансов

### Как работает расчёт

1. **При создании расхода:**
   - Сумма делится поровну между всеми участниками из `participantIds`
   - Если расход 1000₽ на 2 участников → каждый должен 500₽
   - Если расход 3000₽ на 3 участников → каждый должен 1000₽

2. **Расчёт баланса для каждого участника:**
   - `paidAmount` - сумма всех расходов, где пользователь был плательщиком (реально заплатил)
   - `owedAmount` - сумма, которую пользователь должен (сумма всех расходов, где он участник, деленная на количество участников)
   - `balance = paidAmount - owedAmount`

3. **Интерпретация баланса:**
   - **Положительный баланс** → пользователю должны деньги (он переплатил)
   - **Отрицательный баланс** → пользователь должен деньги (он недоплатил)
   - **Нулевой баланс** → все расчеты сбалансированы

### Как понять кто кому должен (примеры)

#### Пример 1: Два участника

**Расходы:**
- Игорь Синяк заплатил за такси: 1000₽ (участники: Игорь Синяк, Мария Вэй)
- Мария Вэй заплатила за обед: 2000₽ (участники: Игорь Синяк, Мария Вэй)

**Расчёт:**
- Игорь Синяк: заплатил 1000₽, должен 1500₽ (1000/2 + 2000/2) → баланс = -500₽
- Мария Вэй: заплатила 2000₽, должна 1500₽ (1000/2 + 2000/2) → баланс = +500₽

**Вывод:** Игорь Синяк должен Марии Вэй 500₽

#### Пример 2: Три участника

**Расходы:**
- Игорь Синяк заплатил за отель: 6000₽ (участники: Игорь Синяк, Мария Вэй, Валентин Петухов)
- Мария Вэй заплатила за обед: 3000₽ (участники: Игорь Синяк, Мария Вэй, Валентин Петухов)
- Валентин Петухов заплатил за такси: 1500₽ (участники: Игорь Синяк, Мария Вэй, Валентин Петухов)

**Расчёт:**
- Игорь Синяк: заплатил 6000₽, должен 3500₽ (6000/3 + 3000/3 + 1500/3) → баланс = +2500₽
- Мария Вэй: заплатила 3000₽, должна 3500₽ → баланс = -500₽
- Валентин Петухов: заплатил 1500₽, должен 3500₽ → баланс = -2000₽

**Вывод:**
- Мария Вэй должна Игорю Синяку 500₽
- Валентин Петухов должен Игорю Синяку 2000₽

#### Пример 3: Последовательные расходы двух участников

**Расходы (все на двоих - Игорь Синяк и Мария Вэй):**
1. Игорь Синяк заплатил: 5800₽ (участники: Игорь Синяк, Мария Вэй)
2. Мария Вэй заплатила: 3400₽ (участники: Игорь Синяк, Мария Вэй)
3. Игорь Синяк заплатил: 2100₽ (участники: Игорь Синяк, Мария Вэй)

**Пошаговый расчёт:**

**Расход 1 (5800₽ на двоих):**
- Каждый должен: 5800₽ ÷ 2 = 2900₽
- Игорь Синяк заплатил 5800₽, но должен только 2900₽ → переплатил 2900₽
- Мария Вэй должна 2900₽

**Расход 2 (3400₽ на двоих):**
- Каждый должен: 3400₽ ÷ 2 = 1700₽
- Мария Вэй заплатила 3400₽, но должна только 1700₽ → переплатила 1700₽
- Игорь Синяк должен 1700₽

**Расход 3 (2100₽ на двоих):**
- Каждый должен: 2100₽ ÷ 2 = 1050₽
- Игорь Синяк заплатил 2100₽, но должен только 1050₽ → переплатил 1050₽
- Мария Вэй должна 1050₽

**Итоговый баланс:**

- **Игорь Синяк:**
  - Заплатил всего: 5800₽ + 2100₽ = **7900₽**
  - Должен всего: 2900₽ (расход 1) + 1700₽ (расход 2) + 1050₽ (расход 3) = **5650₽**
  - **Баланс: 7900₽ - 5650₽ = +2250₽** ✅ (ему должны)

- **Мария Вэй:**
  - Заплатила всего: **3400₽**
  - Должна всего: 2900₽ (расход 1) + 1700₽ (расход 2) + 1050₽ (расход 3) = **5650₽**
  - **Баланс: 3400₽ - 5650₽ = -2250₽** ❌ (она должна)

**Вывод:** Мария Вэй должна Игорю Синяку **2250₽**

**Проверка:** 2250₽ (долг Марии Вэй) = 2250₽ (кредит Игоря Синяка) ✅ Суммы сходятся!

#### Пример 4: Сложный сценарий с тремя участниками

**Расходы:**
- Игорь Синяк заплатил за отель: 12000₽ (участники: Игорь Синяк, Мария Вэй, Валентин Петухов)
- Мария Вэй заплатила за обед: 3000₽ (участники: Игорь Синяк, Мария Вэй)
- Валентин Петухов заплатил за такси: 1500₽ (участники: Игорь Синяк, Мария Вэй, Валентин Петухов)

**Расчёт:**
- Игорь Синяк: 
  - Заплатил: 12000₽
  - Должен: 12000/3 (отель) + 3000/2 (обед) + 1500/3 (такси) = 4000 + 1500 + 500 = 6000₽
  - Баланс: +6000₽
  
- Мария Вэй:
  - Заплатила: 3000₽
  - Должна: 12000/3 + 3000/2 + 1500/3 = 4000 + 1500 + 500 = 6000₽
  - Баланс: -3000₽

- Валентин Петухов:
  - Заплатил: 1500₽
  - Должен: 12000/3 + 1500/3 = 4000 + 500 = 4500₽
  - Баланс: -3000₽

**Вывод:**
- Мария Вэй должна Игорю Синяку 3000₽
- Валентин Петухов должен Игорю Синяку 3000₽

#### Пример 5: Когда Игорь Синяк должен деньги

**Расходы (все на троих - Игорь Синяк, Мария Вэй, Валентин Петухов):**
- Мария Вэй заплатила за отель: 12000₽ (участники: Игорь Синяк, Мария Вэй, Валентин Петухов)
- Валентин Петухов заплатил за обед: 6000₽ (участники: Игорь Синяк, Мария Вэй, Валентин Петухов)
- Игорь Синяк заплатил за такси: 1500₽ (участники: Игорь Синяк, Мария Вэй, Валентин Петухов)

**Расчёт:**
- **Игорь Синяк:**
  - Заплатил: 1500₽
  - Должен: 12000/3 + 6000/3 + 1500/3 = 4000 + 2000 + 500 = **6500₽**
  - **Баланс: 1500₽ - 6500₽ = -5000₽** ❌ (он должен)

- **Мария Вэй:**
  - Заплатила: 12000₽
  - Должна: 12000/3 + 6000/3 + 1500/3 = 4000 + 2000 + 500 = **6500₽**
  - **Баланс: 12000₽ - 6500₽ = +5500₽** ✅ (ей должны)

- **Валентин Петухов:**
  - Заплатил: 6000₽
  - Должен: 12000/3 + 6000/3 + 1500/3 = 4000 + 2000 + 500 = **6500₽**
  - **Баланс: 6000₽ - 6500₽ = -500₽** ❌ (он должен)

**Вывод:**
- **Игорь Синяк должен Марии Вэй 5000₽**
- **Валентин Петухов должен Марии Вэй 500₽**

### Как понять, кому должен пользователь с отрицательным балансом

Когда у пользователя **отрицательный баланс**, это означает, что он должен деньги. Чтобы понять **кому именно**, нужно:

1. **Посмотреть на всех участников поездки:**
   ```bash
   GET /api/trips/{tripId}/balance
   ```

2. **Найти участников с положительным балансом** - это те, кому должны деньги

3. **Алгоритм распределения долгов:**
   - Должник (отрицательный баланс) должен кредитору (положительный баланс)
   - Обычно долг распределяется на кредитора с наибольшим положительным балансом
   - Если у нескольких кредиторов положительный баланс, долг можно распределить пропорционально

**Пример из системы:**

```bash
curl -X GET http://localhost:5001/api/trips/TRIP_ID/balance \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**Ответ (когда Игорь Синяк должен):**
```json
[
  {
    "userId": "sinyak-id",
    "userName": "Игорь Синяк",
    "paidAmount": 1500.00,
    "owedAmount": 6500.00,
    "balance": -5000.00
  },
  {
    "userId": "wei-id",
    "userName": "Мария Вэй",
    "paidAmount": 12000.00,
    "owedAmount": 6500.00,
    "balance": 5500.00
  },
  {
    "userId": "petukhov-id",
    "userName": "Валентин Петухов",
    "paidAmount": 6000.00,
    "owedAmount": 6500.00,
    "balance": -500.00
  }
]
```

**Как интерпретировать:**

1. **Игорь Синяк (balance: -5000₽)** - он должен деньги
   - Смотрим на кредиторов: Мария Вэй (+5500₽)
   - **Игорь Синяк должен Марии Вэй 5000₽**

2. **Валентин Петухов (balance: -500₽)** - он должен деньги
   - Смотрим на кредиторов: Мария Вэй (+5500₽, после получения от Игоря будет +500₽)
   - **Валентин Петухов должен Марии Вэй 500₽**

3. **Мария Вэй (balance: +5500₽)** - ей должны деньги
   - Игорь Синяк должен ей 5000₽
   - Валентин Петухов должен ей 500₽
   - **Итого ей должны: 5500₽** ✅

**Визуализация в системе:**

Система показывает баланс каждого участника. Чтобы понять, кому должен конкретный человек:

- **Если баланс отрицательный** → этот человек должен деньги
- **Если баланс положительный** → этому человеку должны деньги
- **Сумма всех отрицательных балансов = сумме всех положительных балансов** (всегда!)

**Правило:** Должник (отрицательный баланс) должен кредитору (положительный баланс). Обычно долг идет кредитору с наибольшим положительным балансом.

### Алгоритм определения долгов

1. **Получите балансы всех участников:**
   ```bash
   GET /api/trips/{tripId}/balance
   ```

2. **Разделите участников на две группы:**
   - **Кредиторы** (положительный баланс) - им должны деньги
   - **Должники** (отрицательный баланс) - они должны деньги

3. **Для каждого должника:**
   - Найдите кредитора с наибольшим положительным балансом
   - Переведите минимальную сумму (между долгом и кредитом)
   - Повторяйте, пока все балансы не станут нулевыми

**Пример расчета переводов для примера 4:**
- Мария Вэй (-3000₽) → Игорь Синяк (+6000₽): перевод 3000₽
- Валентин Петухов (-3000₽) → Игорь Синяк (+3000₽): перевод 3000₽
- Итог: все балансы = 0₽

### Практический пример использования

```bash
# Получить балансы
curl -X GET http://localhost:5001/api/trips/TRIP_ID/balance \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**Ответ:**
```json
[
  {
    "userId": "sinyak-id",
    "userName": "Игорь Синяк",
    "paidAmount": 12000.00,
    "owedAmount": 6000.00,
    "balance": 6000.00
  },
  {
    "userId": "wei-id",
    "userName": "Мария Вэй",
    "paidAmount": 3000.00,
    "owedAmount": 6000.00,
    "balance": -3000.00
  },
  {
    "userId": "petukhov-id",
    "userName": "Валентин Петухов",
    "paidAmount": 1500.00,
    "owedAmount": 4500.00,
    "balance": -3000.00
  }
]
```

**Интерпретация:**
- Игорь Синяк переплатил 6000₽ → ему должны вернуть деньги
- Мария Вэй недоплатила 3000₽ → она должна доплатить
- Валентин Петухов недоплатил 3000₽ → он должен доплатить

**Кто кому должен:**
- Мария Вэй → Игорю Синяку: 3000₽
- Валентин Петухов → Игорю Синяку: 3000₽

### Как увидеть, кто кому должен (новый эндпоинт)

Система теперь показывает **конкретные долги** между участниками:

#### Вариант 1: Все долги по поездке

```bash
curl -X GET http://localhost:5001/api/trips/TRIP_ID/debts \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**Ответ:**
```json
[
  {
    "fromUserId": "wei-id",
    "fromUserName": "Мария Вэй",
    "toUserId": "sinyak-id",
    "toUserName": "Игорь Синяк",
    "amount": 3000.00
  },
  {
    "fromUserId": "petukhov-id",
    "fromUserName": "Валентин Петухов",
    "toUserId": "sinyak-id",
    "toUserName": "Игорь Синяк",
    "amount": 3000.00
  }
]
```

**Интерпретация:**
- Мария Вэй должна Игорю Синяку 3000₽
- Валентин Петухов должен Игорю Синяку 3000₽

#### Вариант 2: Мои долги (для текущего пользователя)

```bash
curl -X GET http://localhost:5001/api/trips/TRIP_ID/my-debts \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**Ответ (если вы Игорь Синяк):**
```json
{
  "userId": "sinyak-id",
  "userName": "Игорь Синяк",
  "totalDebt": 0.00,
  "totalCredit": 6000.00,
  "debts": [],
  "credits": [
    {
      "fromUserId": "wei-id",
      "fromUserName": "Мария Вэй",
      "toUserId": "sinyak-id",
      "toUserName": "Игорь Синяк",
      "amount": 3000.00
    },
    {
      "fromUserId": "petukhov-id",
      "fromUserName": "Валентин Петухов",
      "toUserId": "sinyak-id",
      "toUserName": "Игорь Синяк",
      "amount": 3000.00
    }
  ]
}
```

**Интерпретация:**
- Вам должны 6000₽ всего
- Мария Вэй должна вам 3000₽
- Валентин Петухов должен вам 3000₽

**Ответ (если вы Мария Вэй и должны деньги):**
```json
{
  "userId": "wei-id",
  "userName": "Мария Вэй",
  "totalDebt": 3000.00,
  "totalCredit": 0.00,
  "debts": [
    {
      "fromUserId": "wei-id",
      "fromUserName": "Мария Вэй",
      "toUserId": "sinyak-id",
      "toUserName": "Игорь Синяк",
      "amount": 3000.00
    }
  ],
  "credits": []
}
```

**Интерпретация:**
- Вы должны 3000₽ всего
- Вы должны Игорю Синяку 3000₽

#### Вариант 3: Долги конкретного пользователя

```bash
curl -X GET http://localhost:5001/api/trips/TRIP_ID/debts/USER_ID \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**Ответ такой же, как в варианте 2, но для указанного пользователя.**

#### Вариант 4: Мои долги (для текущего пользователя)

```bash
curl -X GET http://localhost:5001/api/trips/TRIP_ID/my-debts \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**Ответ такой же, как в варианте 3, но для текущего авторизованного пользователя.**

### Пример: Когда Игорь Синяк должен деньги

**Расходы:**
- Мария Вэй заплатила: 12000₽
- Валентин Петухов заплатил: 6000₽
- Игорь Синяк заплатил: 1500₽

**Запрос:**
```bash
curl -X GET http://localhost:5001/api/trips/TRIP_ID/debts/SINYAK_USER_ID \
  -H "Authorization: Bearer SINYAK_JWT_TOKEN"
```

Или используйте эндпоинт для текущего пользователя:

```bash
curl -X GET http://localhost:5001/api/trips/TRIP_ID/my-debts \
  -H "Authorization: Bearer SINYAK_JWT_TOKEN"
```

**Ответ (для Игоря Синяка):**
```json
{
  "userId": "sinyak-id",
  "userName": "Игорь Синяк",
  "totalDebt": 5000.00,
  "totalCredit": 0.00,
  "debts": [
    {
      "fromUserId": "sinyak-id",
      "fromUserName": "Игорь Синяк",
      "toUserId": "wei-id",
      "toUserName": "Мария Вэй",
      "amount": 5000.00
    }
  ],
  "credits": []
}
```

**Интерпретация:**
- Вы должны 5000₽ всего
- **Вы должны Марии Вэй 5000₽** ← вот это и есть ответ на вопрос "кому я должен"

**Запрос для Марии Вэй:**
```bash
curl -X GET http://localhost:5001/api/trips/TRIP_ID/debts/WEI_USER_ID \
  -H "Authorization: Bearer WEI_JWT_TOKEN"
```

Или используйте эндпоинт для текущего пользователя:

```bash
curl -X GET http://localhost:5001/api/trips/TRIP_ID/my-debts \
  -H "Authorization: Bearer WEI_JWT_TOKEN"
```

**Ответ (для Марии Вэй):**
```json
{
  "userId": "wei-id",
  "userName": "Мария Вэй",
  "totalDebt": 0.00,
  "totalCredit": 5500.00,
  "debts": [],
  "credits": [
    {
      "fromUserId": "sinyak-id",
      "fromUserName": "Игорь Синяк",
      "toUserId": "wei-id",
      "toUserName": "Мария Вэй",
      "amount": 5000.00
    },
    {
      "fromUserId": "petukhov-id",
      "fromUserName": "Валентин Петухов",
      "toUserId": "wei-id",
      "toUserName": "Мария Вэй",
      "amount": 500.00
    }
  ]
}
```

**Интерпретация:**
- Вам должны 5500₽ всего
- Игорь Синяк должен вам 5000₽
- Валентин Петухов должен вам 500₽

## Обработка ошибок

Все ошибки возвращаются в едином формате:

```json
{
  "error": "Сообщение об ошибке",
  "statusCode": 404
}
```

**HTTP коды:**
- `200 OK` - успешный запрос
- `201 Created` - ресурс создан
- `204 No Content` - ресурс удален
- `400 Bad Request` - неверный запрос
- `401 Unauthorized` - требуется авторизация
- `403 Forbidden` - недостаточно прав
- `404 Not Found` - ресурс не найден
- `500 Internal Server Error` - внутренняя ошибка сервера

## Rate Limiting

Ограничение: **100 запросов в минуту** на IP адрес.

При превышении лимита вернется статус `429 Too Many Requests`.

## Кэширование

- **GET /api/trips/{id}** кэшируется в Redis на 5 минут
- Кэш автоматически инвалидируется при обновлении или удалении поездки

## Миграции базы данных

База данных создается автоматически через **Liquibase** при первом запуске Docker Compose.

Структура миграций:
```
liquibase/
├── changelog/
│   ├── changelog-master.xml
│   └── 001-initial-schema.xml
└── liquibase.properties
```

## Конфигурация

### Настройка приложения

**⚠️ Важно:** Не коммитьте `appsettings.json` с реальными паролями в Git!

1. **Скопируйте пример конфигурации:**
   ```bash
   cp Tripwithfriends/appsettings.Example.json Tripwithfriends/appsettings.json
   ```

2. **Отредактируйте `appsettings.json`** и укажите свои значения:
   - `ConnectionStrings:DefaultConnection` - строка подключения к PostgreSQL
   - `ConnectionStrings:Redis` - строка подключения к Redis
   - `Jwt:Key` - секретный ключ для JWT (минимум 32 символа!)

3. **Для Docker Compose** используйте переменные окружения или `.env` файл (не коммитьте в Git!)

### Структура конфигурации

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=tripwithfriends;Username=postgres;Password=YOUR_PASSWORD",
    "Redis": "localhost:6379"
  },
  "Jwt": {
    "Key": "YOUR_SECRET_KEY_AT_LEAST_32_CHARACTERS_LONG!",
    "Issuer": "Tripwithfriends",
    "Audience": "Tripwithfriends",
    "ExpirationMinutes": "60"
  }
}
```

**Подробные инструкции по настройке:** [SETUP.md](SETUP.md)

## Запуск тестов

```bash
# Запустить все unit-тесты
dotnet test Tripwithfriends.Tests/Tripwithfriends.Tests.csproj

# С подробным выводом
dotnet test --verbosity normal

# С покрытием кода (если установлен coverlet)
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Структура проекта

```
Tripwithfriends/
├── Controllers/          # API контроллеры
├── Services/            # Бизнес-логика
│   └── Interfaces/      # Интерфейсы сервисов
├── Repositories/        # Доступ к данным
│   └── Interfaces/      # Интерфейсы репозиториев
├── Data/                # DbContext
├── Models/
│   └── Entities/        # Сущности базы данных
├── DTO/                 # Data Transfer Objects
├── Validators/          # FluentValidation валидаторы
├── Middleware/          # Middleware (ошибки, idempotency)

Tripwithfriends.Tests/
└── Repositories/        # Unit-тесты репозиториев

liquibase/
└── changelog/           # Миграции базы данных
```

## Примеры использования в разных языках

### JavaScript (Fetch API)

```javascript
// Регистрация
const response = await fetch('http://localhost:5001/api/auth/register', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    name: 'Игорь Синяк',
    email: 'sinyak@instagram.com',
    password: 'CrazyRussian2024!',
    role: 'User'
  })
});
const user = await response.json();

// Вход
const loginResponse = await fetch('http://localhost:5001/api/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    email: 'sinyak@instagram.com',
    password: 'CrazyRussian2024!'
  })
});
const { token } = await loginResponse.json();

// Создание поездки
const tripResponse = await fetch('http://localhost:5001/api/trips', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`
  },
  body: JSON.stringify({
    name: 'Поездка в Сочи',
    startDate: '2026-07-01T00:00:00Z',
    endDate: '2026-07-10T00:00:00Z',
    participantIds: []
  })
});
const trip = await tripResponse.json();
```

### Python (requests)

```python
import requests

BASE_URL = 'http://localhost:5001'

# Регистрация
response = requests.post(f'{BASE_URL}/api/auth/register', json={
    'name': 'Игорь Синяк',
    'email': 'sinyak@instagram.com',
    'password': 'CrazyRussian2024!',
    'role': 'User'
})
user = response.json()

# Вход
response = requests.post(f'{BASE_URL}/api/auth/login', json={
    'email': 'sinyak@instagram.com',
    'password': 'CrazyRussian2024!'
})
token = response.json()['token']

# Создание поездки
headers = {'Authorization': f'Bearer {token}'}
response = requests.post(f'{BASE_URL}/api/trips', 
    headers=headers,
    json={
        'name': 'Поездка в Сочи',
        'startDate': '2026-07-01T00:00:00Z',
        'endDate': '2026-07-10T00:00:00Z',
        'participantIds': []
    })
trip = response.json()
```
