# Payment Gateway Integration System

Web-based payment processing system integrating external payment provider checkout with return flow and webhook notification handling.

Built with ASP.NET Web Forms (.NET Framework 4.7.2/4.8) and SQL Server.

## 🚀 Overview

This project demonstrates a complete payment integration flow:

1. Payment initiation from the application
2. Redirect to external provider checkout
3. Return URL handling after payment
4. Webhook endpoint to receive asynchronous payment notifications
5. Transaction persistence and status updates in SQL Server

Sensitive credentials and production configuration values have been removed for security reasons.

## 🛠 Tech Stack

- ASP.NET Web Forms
- .NET Framework 4.7.2 / 4.8
- SQL Server
- IIS

## 📂 Project Structure

- **Payment.WebApp**  
  Main application handling:
  - Payment token generation
  - Checkout redirection
  - Payment result page
  - Transaction logging

- **Payment.Webhook**
  - Receives POST notifications from payment provider
  - Validates request
  - Updates transaction status in database

- **Payment.Api (JSON Service)**
  - Auxiliary API service for integration support

## 🔄 Payment Flow

1. User initiates payment
2. System generates checkout token
3. User is redirected to external payment provider
4. Provider redirects back to result page
5. Webhook receives final payment confirmation
6. Database updates transaction status

## ⚙️ Configuration

Update `Web.config` in each project:

### ConnectionStrings
- Server
- Database
- User Id / Password (or Integrated Security)

### Payment Provider Settings
- ClientId
- ClientSecret
- NotificationUrl
- ReturnUrl

⚠️ Do not commit real credentials to the repository.

## 🔒 Security Considerations

- Secure handling of payment tokens
- Server-side validation of webhook notifications
- Database transaction integrity
- Separation between public endpoints and internal logic

## 📌 Notes

This repository contains a sanitized version of a real-world payment integration.  
Sensitive business logic and production credentials have been intentionally removed.

## 📄 License

MIT
