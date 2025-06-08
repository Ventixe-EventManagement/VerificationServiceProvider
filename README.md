# VerificationServiceProvider

**VerificationServiceProvider** is a backend microservice responsible for sending and verifying email-based verification codes. It is a core part of the user identity and authentication workflow in the Ventixe platform. The service is built using **ASP.NET Core** and integrates with **Azure Communication Services** and **Azure Key Vault** to ensure secure and reliable delivery.

## Overview

This service provides a RESTful API to support email verification functionality. It can send one-time verification codes and validate codes submitted by users. All operations are authenticated, and sensitive configuration values are securely retrieved from **Azure Key Vault**.

## Features

- Send email verification codes using **Azure Communication Services (ACS)**
- Verify submitted email verification codes
- Configurable code lifetime and retry limits (service-level logic)
- Secrets managed securely via **Azure Key Vault**
- Designed to integrate into larger user authentication and account services

## Technology Stack

- **ASP.NET Core 9**  
- **Azure Communication Services (Email)**  
- **Azure Key Vault**  
- **Azure App Service**  
- **Azure Identity** (for `DefaultAzureCredential`)  

## Azure Key Vault Integration

All secrets and connection strings are loaded at runtime from **Azure Key Vault** using a **system-assigned managed identity**. This ensures that no sensitive values are hardcoded or stored in plaintext.

### Secrets used

| Key                     | Purpose                                          |
|--------------------------|--------------------------------------------------|
| `KeyVault:Url`           | URI to the Azure Key Vault                      |
| `ACS:ConnectionString`   | Connection string for Azure Communication Email |

