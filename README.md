# Venta - Microservicios con JWT Authentication

Sistema de microservicios desarrollado en .NET 8 con autenticación JWT para una aplicación de ventas.

## Arquitectura

El proyecto está compuesto por 4 microservicios independientes:

### 🔐 UsersService (Puerto 5110)
- **Funcionalidad**: Autenticación y gestión de usuarios
- **Endpoints principales**:
  - `POST /auth/login` - Iniciar sesión
  - `POST /auth/register` - Registrar nuevo usuario
  - `GET /users/profile` - Obtener perfil del usuario actual
  - `GET /users` - Listar todos los usuarios (Solo Admin)

### 📦 ProductsService (Puerto 5217)
- **Funcionalidad**: Gestión de productos
- **Endpoints principales**:
  - `GET /products` - Listar productos (Público)
  - `GET /products/{id}` - Obtener producto por ID (Protegido)
  - `POST /products` - Crear nuevo producto (Protegido)

### 💳 PaymentsService (Puerto 5037)
- **Funcionalidad**: Procesamiento de pagos
- **Endpoints principales**:
  - `GET /payments` - Listar pagos (Protegido)
  - `GET /payments/{id}` - Obtener pago por ID (Protegido)
  - `POST /payments` - Crear nuevo pago (Protegido)
  - `PUT /payments/{id}/status` - Actualizar estado del pago (Protegido)

### 📋 LogsService (Puerto 5299)
- **Funcionalidad**: Sistema de logging centralizado
- **Endpoints principales**:
  - `GET /logs` - Obtener todos los logs (Solo Admin)
  - `GET /logs/service/{service}` - Logs por servicio (Solo Admin)
  - `GET /logs/level/{level}` - Logs por nivel (Solo Admin)
  - `POST /logs` - Crear entrada de log (Protegido)

## 🚀 Tecnologías Utilizadas

- **.NET 8.0** - Framework principal
- **ASP.NET Core Web API** - Para crear los microservicios
- **JWT Bearer Authentication** - Para autenticación y autorización
- **Microsoft.IdentityModel.Tokens** - Manejo de tokens JWT

## 🔧 Configuración

### Requisitos Previos
- .NET 8.0 SDK
- Visual Studio Code o Visual Studio

### Configuración JWT
Todos los servicios utilizan la misma configuración JWT para validar tokens:

```json
{
  "Jwt": {
    "Key": "ThisIsASecretKeyForJWTTokenGeneration123456789",
    "Issuer": "UsersService",
    "Audience": "VentaApp",
    "ExpireMinutes": 60
  }
}
```

## 📖 Instalación y Ejecución

### 1. Clonar el repositorio
```bash
git clone <repository-url>
cd Venta
```

### 2. Ejecutar los servicios
Abrir 4 terminales y ejecutar cada servicio:

```bash
# Terminal 1 - UsersService
cd UsersService
dotnet run

# Terminal 2 - ProductsService  
cd ProductsService
dotnet run

# Terminal 3 - PaymentsService
cd PaymentsService
dotnet run

# Terminal 4 - LogsService
cd LogsService
dotnet run
```

### 3. Acceder a la documentación Swagger
- UsersService: https://localhost:7001/swagger
- ProductsService: https://localhost:7002/swagger
- PaymentsService: https://localhost:7003/swagger
- LogsService: https://localhost:7004/swagger

## 🧪 Pruebas con archivos HTTP

Cada servicio incluye un archivo `.http` con ejemplos de requests:
- `UsersService/UsersService.http`
- `ProductsService/ProductsService.http`
- `PaymentsService/PaymentsService.http`
- `LogsService/LogsService.http`

## 👤 Usuarios Predefinidos

El sistema incluye usuarios de prueba:

| Username | Password | Role | Descripción |
|----------|----------|------|-------------|
| admin | password123 | Admin | Acceso completo a todos los servicios |
| user1 | password123 | User | Acceso limitado según permisos |
| user2 | password123 | User | Acceso limitado según permisos |

## 🔑 Flujo de Autenticación

1. **Login**: Hacer POST a `/auth/login` en UsersService
2. **Obtener Token**: El servicio retorna un JWT token
3. **Usar Token**: Incluir el token en el header `Authorization: Bearer {token}` para endpoints protegidos

Ejemplo:
```bash
# 1. Login
POST https://localhost:7001/auth/login
Content-Type: application/json

{
    "username": "admin",
    "password": "password123"
}

# 2. Usar el token recibido
GET https://localhost:7002/products/1
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## 🏗️ Estructura del Proyecto

```
Venta/
├── Venta.sln
├── UsersService/
│   ├── Program.cs
│   ├── appsettings.json
│   └── UsersService.http
├── ProductsService/
│   ├── Program.cs
│   ├── appsettings.json
│   └── ProductsService.http
├── PaymentsService/
│   ├── Program.cs
│   ├── appsettings.json
│   └── PaymentsService.http
└── LogsService/
    ├── Program.cs
    ├── appsettings.json
    └── LogsService.http
```

## 📝 Notas de Desarrollo

- **Seguridad**: En producción, cambiar la clave JWT y usar variables de entorno
- **Base de Datos**: Actualmente usa datos en memoria, implementar persistencia según necesidades
- **CORS**: Configurar CORS si se necesita acceso desde aplicaciones web
- **Logging**: El LogsService proporciona logging centralizado para todos los microservicios

## 🤝 Contribuir

1. Fork el proyecto
2. Crear una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abrir un Pull Request