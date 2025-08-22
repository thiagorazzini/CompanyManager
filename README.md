# Company Manager API

This project provides a RESTful API built with ASP.NET Core (.NET 8), using SQL Server and Docker for backend infrastructure. It implements authentication via JWT and follows a clean architecture with DDD principles for complete company, department, employee, and job title management.

---

## 🛠️ Setup

Before running, ensure the project builds correctly:

```bash
dotnet clean
dotnet build
```

Then run the full environment with Docker:

```bash
docker-compose up --build
```

The API will be available at:

```
http://localhost:5000/swagger
```

The Frontend will be available at:

```
http://localhost:3000
```

---

## 👤 Creating a User

Use the following cURL command to create your first user:

```bash
curl -X 'POST' \
  'http://localhost:5000/api/Auth/register' \
  -H 'accept: text/plain' \
  -H 'Content-Type: application/json' \
  -d '{
  "username": "Admin",
  "password": "Admin@123456",
  "email": "admin@company.com",
  "role": "SuperUser"
}'
```

---

## 🔐 Authentication

After creating the user, obtain a JWT token via:

```bash
curl -X 'POST' \
  'http://localhost:5000/api/Auth/authenticate' \
  -H 'accept: text/plain' \
  -H 'Content-Type: application/json' \
  -d '{
  "email": "admin@company.com",
  "password": "Admin@123456"
}'
```

### ✅ Response Example

```json
{
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "email": "admin@company.com",
    "username": "Admin",
    "role": "SuperUser"
  },
  "success": true,
  "message": "User authenticated successfully",
  "errors": []
}
```

---

## 🔓 Using the Token

After authenticating, you can call protected endpoints. In Swagger, use **"Authorize"** at the top and insert:

```
Bearer <your_token>
```

Or via cURL, access protected resources:

```bash
curl -X 'GET' \
  'http://localhost:5000/api/Departments' \
  -H 'accept: text/plain' \
  -H 'Authorization: Bearer <your_token>'
```

---

## 📊 Main Features

### 🏢 Department Management
- Create, edit, list and delete departments
- Organizational hierarchy
- Filters and pagination

### 👥 Employee Management
- Complete employee registration
- Association with departments and job titles
- Organizational hierarchy validation
- Role-based access control

### 💼 Job Title Management
- Definition of job titles and responsibilities
- Salary structure
- Hierarchical levels

### 🔐 Access Control
- JWT Authentication
- Hierarchical authorization
- Role and permission management

---

## 🏗️ Architecture

### Backend (.NET 8)
- **Framework:** ASP.NET Core 8
- **Database:** SQL Server 2022
- **ORM:** Entity Framework Core 8
- **Authentication:** JWT Bearer Tokens
- **Architecture:** Clean Architecture + DDD
- **Patterns:** CQRS with MediatR
- **Validation:** FluentValidation
- **Mapping:** AutoMapper
- **Testing:** xUnit

### Frontend (React)
- **Framework:** React 18
- **Language:** TypeScript
- **Build Tool:** Vite
- **Styling:** Tailwind CSS
- **Routing:** React Router DOM
- **HTTP Client:** Axios
- **Icons:** Heroicons
- **Notifications:** React Hot Toast


---

## 📦 Tech Stack

### Backend
- ASP.NET Core 8
- SQL Server 2022
- Entity Framework Core 8
- Docker
- MediatR (CQRS)
- AutoMapper
- FluentValidation
- JWT Authentication
- DDD + Clean Architecture

### Frontend
- React 18
- TypeScript
- Vite
- Tailwind CSS
- React Router DOM
- Axios
- Heroicons
- React Hot Toast

---

## 🧪 Usage Tips

- Always create a user before authenticating
- Use Swagger to explore all endpoints visually
- JWT is required for most protected endpoints
- Organizational hierarchy is automatically respected
- Employees can only manage direct subordinates
- Frontend runs on port 3000, API on port 5000
- Database management available via Adminer on port 8080

---

## 📸 Evidence

Below are screenshots confirming complete execution and validation of the system:

### ✅ 1. Docker Compose Running
![Docker Running](.doc/docker_compose_running.png)

### ➕ 2. Create User
![Create User](.doc/create_user.png)


### 📥 3. Validate Token
![Validate Token](.doc/validate_token.png)

### 🧪 4. Add Bearer Token to Swagger Authorize
![Bearer in Swagger](.doc/add_bearer_token_to_swagger_authorize.png)

### 📦 5. Get Departments with Token
![Get Departments](.doc/get_departments_with_token.png)

### 🔄 6. Swagger Interface Active
![Swagger](.doc/swagger_running.png)

### ✅ 7. Unit Tests Successful
![Test Explorer](.doc/test_explorer_running_successfull.png)

---

## 🖥️ Frontend Interface

### 🔐 8. Login Screen
![Login](.doc/front_login.png)

### 🏠 9. Main Dashboard
![Dashboard](.doc/front_dash.png)

### 👤 10. User Creation via Frontend
![Create User Frontend](.doc/create_user_front.png)

### 🏢 11. Department Listing
![List Departments](.doc/front_list_department.png)

### ➕ 12. Department Creation
![Create Department](.doc/front_create_department.png)

### ✏️ 13. Department Editing
![Edit Department](.doc/front_edit_department.png)

### 👥 14. Employee Listing
![List Employees](.doc/front_list_employee.png)

### ➕ 15. Employee Creation
![Create Employee](.doc/front_create_employee.png)

### ✏️ 16. Employee Editing
![Edit Employee](.doc/front_edit_employee.png)

---

