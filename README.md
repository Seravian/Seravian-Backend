# Seravian – Backend

This repository contains the **ASP.NET Core Web API backend** for **Seravian**, a mental health support platform developed as a **graduation project** by Computer Science students (2024–2025).

The backend serves as a secure and intelligent middleware layer between mobile/web clients and external AI services deployed on **Modal** (via **FastAPI**). It handles authentication, session management, file processing, and communication with AI models for mental health analysis and emotional diagnostics.

---

## 🛠️ Tech Stack

- **Language:** C# (.NET 9)
- **Framework:** ASP.NET Core Web API
- **Authentication:** JWT + Email OTP
- **Database:** SQL Server + Entity Framework Core
- **Validation:** FluentValidation
- **Email Delivery:** MailKit (SMTP client library for .NET)
- **AI Integration:** FastAPI microservices hosted on Modal
- **Media Handling:** FFmpeg (via bundled binaries per OS)
- **File Storage:** Local filesystem (configurable for cloud)
- **Deployment:** IIS, Docker, Azure (future)
- **Hosting Provider:** MonsterASP (Windows hosting)

---

## 💻 Prerequisites

To build and run this project, make sure the following dependencies are installed:

- ✅ [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- ✅ [FFmpeg](https://ffmpeg.org/download.html)
  - Windows: [https://www.gyan.dev/ffmpeg/builds/](https://www.gyan.dev/ffmpeg/builds/)
  - Linux/macOS: `sudo apt install ffmpeg` or `brew install ffmpeg`
- ✅ [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- ✅ [Visual Studio 2022+](https://visualstudio.microsoft.com/vs/) or [VS Code](https://code.visualstudio.com/) + C# Dev Kit
- ✅ [EF Core CLI Tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)

  ```bash
  dotnet tool install --global dotnet-ef
  ```

## 🧪 Running Locally

1. **Clone the repo:**

   ```bash
   git clone https://github.com/seravian-org/Seravian-Backend.git
   cd Seravian-Backend
   ```

2. **Restore dependencies:**

```bash
dotnet restore
```

3. **Configure Secrets:**
   Set values via `dotnet user-secrets` or `appsettings.Development.json`

4. **Place FFmpeg binaries:**

   Place the appropriate FFmpeg binaries for your OS in the following folder:

   ```

   Seravian/ffmpeg/
   ├── ffmpeg.exe
   └── ffprobe.exe

   ```

   Make sure both `ffmpeg.exe` and `ffprobe.exe` are included. These binaries are required for audio validation and processing.

   - ✅ On **Windows**, download the zip from [https://www.gyan.dev/ffmpeg/builds/](https://www.gyan.dev/ffmpeg/builds/), extract it, and copy the two files into `Seravian/ffmpeg/`.

   - ✅ On **Linux/macOS**, install using:

     ```bash
     sudo apt install ffmpeg       # for Ubuntu/Debian
     brew install ffmpeg           # for macOS
     ```

     Or place the binaries manually in the `ffmpeg/` directory and ensure they are executable (`chmod +x ffmpeg`).

   > 🧪 These binaries are used by the backend internally via `Process.Start()` to inspect and convert audio files.

5. **Apply Migrations (if needed):**

   ```bash
   dotnet ef database update
   ```

6. **Run the API:**

   ```bash
   dotnet run
   ```

7. **Explore with Swagger:**
   Open your browser to:  
   `https://localhost:{port}/swagger`

---

## 🔧 Configuration

<details>
<summary><strong>🔧 Full Configuration Settings</strong> (click to expand)</summary>

### 🔐 JWT Settings

| Key                                      | Description                                       |
|------------------------------------------|---------------------------------------------------|
| `Jwt:AccessTokenKey`                     | Secret key used to sign JWT access tokens        |
| `Jwt:Issuer`                             | Token issuer                                     |
| `Jwt:Audience`                           | Expected audience of the token                   |
| `Jwt:AccessTokenExpirationMinutes`       | Lifetime of access token (in minutes)            |
| `Jwt:ProfileSetupAccessTokenExpirationMinutes` | Lifetime of access token during profile setup   |
| `Jwt:RefreshTokenExpirationDays`         | Lifetime of refresh token (in days)              |

### 📧 Email Settings

| Key                      | Description                       |
|--------------------------|-----------------------------------|
| `Email:SmtpServer`       | SMTP server used for sending emails |
| `Email:Port`             | SMTP port (usually 587 for TLS)   |
| `Email:SenderEmail`      | System email address (sender)     |
| `Email:SenderName`       | Friendly name for sender emails   |
| `Email:AppPassword`      | App password or SMTP authentication token |

> 📧 Emails are sent using the [MailKit](https://github.com/jstedfast/MailKit) library over SMTP. Make sure your SMTP server supports secure connections and app passwords if required.

### 🤖 AI Services (Modular APIs)

| Key                                      | Description                                  |
|------------------------------------------|----------------------------------------------|
| `AI:LLM:MentalLLaMA7BBaseUrl`            | Endpoint for 7B model base URL (text AI)     |
| `AI:LLM:MentalLLaMA13BBaseUrl`           | Endpoint for 13B model base URL (text AI)    |
| `AI:LLM:GenerateResponseEndpointName`    | Subpath for generating AI chat responses     |
| `AI:LLM:GenerateDiagnosisEndpointName`   | Subpath for generating mental health diagnosis |
| `AI:LLM:ApiKey`                          | API key for LLM requests                     |
| `AI:LLM:ApiKeyHeader`                    | Header name for LLM API key (e.g., `x-api-key`) |

### 🧠 Audio & Emotion Analysis

| Key                                      | Description                              |
|------------------------------------------|------------------------------------------|
| `AI:SERAndSTT:ApiUrl`                    | URL for speech emotion recognition (SER) + speech-to-text (STT) |
| `AI:SERAndSTT:ApiKey`                    | API key for SER/STT service              |
| `AI:SERAndSTT:ApiKeyHeader`              | Header name for the key (e.g., `x-api-key`) |

### 👤 Face-Based Emotion Recognition

| Key                                      | Description                              |
|------------------------------------------|------------------------------------------|
| `AI:DeepFace:ApiUrl`                     | URL for DeepFace API (facial emotion detection) |
| `AI:DeepFace:ApiKey`                     | API key for DeepFace                     |
| `AI:DeepFace:ApiKeyHeader`               | Header name for the key                  |

### 🔊 Text-to-Speech (TTS)

| Key                                      | Description                              |
|------------------------------------------|------------------------------------------|
| `AI:TTS:ApiUrl`                          | URL for TTS API (convert text to voice)  |
| `AI:TTS:ApiKey`                          | API key for TTS                          |
| `AI:TTS:ApiKeyHeader`                    | Header name for the key                  |

</details>

---

## 👤 Author & Ownership

This backend repo is part of the **Seravian** GitHub organization, which includes:

- `Seravian-Frontend` (Angular)
- `Seravian-Backend` (ASP.NET Core)
- `Seravian-Mobile` (Kotlin)
- `Seravian-AI` (FastAPI + Python)

> 📌 **Note:** While the platform is a team project,  
> 🧑‍💻 **this backend repo was fully developed by [Mohamed Saeed](https://github.com/mohamedsaeed138)**.

---

## 📜 License

Elastic License v2.0 – see [`LICENSE`](./LICENSE)
