# ShoppingCartWebAPI

An API for managing a shopping cart, allowing user addition, coupon application, and total price calculation.

## 📌 Technologies Used
- **C#** / **.NET** / **ASP.NET Core**
- **SQL Server**
- **Microsoft.Data.SqlClient**

## 🚀 Features

### 1️⃣ Add User to Cart
**Endpoint:** `POST /Cart/Add`
- **Parameters:** `userName` (Query String)
- **Description:** Adds a user to the shopping cart.
- **Request Example:**
  ```sh
  POST /Cart/Add?userName=JohnDoe
  ```
- **Response:**
  ```json
  "JohnDoe added to cart"
  ```

### 2️⃣ Apply Coupon to Cart
**Endpoint:** `POST /Cart/ApplyCoupon`
- **Description:** Applies a discount coupon to the cart.

### 3️⃣ Complete Cart (Total and Subtotal)
**Endpoint:** `GET /Cart/Complete`
- **Description:** Retrieves cart items, calculates subtotal and total, and returns a JSON with the details.

## 🛠 How to Run the Project

### 🔧 Prerequisites
- .NET SDK installed
- SQL Server configured
- Visual Studio 2022 or VS Code

### 🚀 Steps to Run the API
1. Clone the repository:
   ```sh
   git clone https://github.com/KayoFontesLunga/ShoppingCartWebAPI.git
   ```
2. Navigate to the project directory:
   ```sh
   cd ShoppingCartWebAPI
   ```
3. Configure the connection string in the `appsettings.json` file.
4. Run the project:
   ```sh
   dotnet run
   ```
5. The API will be available at `http://localhost:<port>`

## 📄 License
This project is licensed under the [MIT License](LICENSE).

