

# 🚀 Introduction

Welcome to the **LPS Tool** – your ultimate companion for **Load**, **Performance**, and **Stress** testing!

🛠️ The **LPS Tool** (Load, Performance, and Stress Testing Command Tool) is a flexible framework for testing your web application's performance under simulated load.

### 🌟 Key Highlights
- 🔁 Built on [**Rounds**](https://github.com/mohaidr/lps-docs/blob/main/concepts/1.Rounds.md) and [**Iterations**](https://github.com/mohaidr/lps-docs/blob/main/concepts/2.Iterations.md) for structured testing
- 🎛️ Offers flexible [**Iteration Modes**](https://github.com/mohaidr/lps-docs/blob/main/concepts/3.Iteration_Modes.md) to simulate real-world traffic
- 📊 Helps evaluate system **scalability**, **endurance**, and **resilience**
- ⚙️ Empowers developers and QA engineers with powerful testing scenarios

---

# 💻 Installation Guide

🧭 **LPS Tool is cross-platform** – it works on **Windows**, **Linux**, and **macOS**!

## 🛠️ Quick Install (Recommended)

You can now install the **LPS Tool** directly from **NuGet** as a global .NET CLI tool:

```bash
dotnet tool install --global lps
```

✅ **Requirements:**  
Make sure you have [.NET 8 SDK or Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) installed on your machine.

After installation, you can run LPS from anywhere using:

```bash
lps --version
```



# ⚡ Quick Test Examples

### 1️⃣ Simple GET Request
```bash
lps --url https://www.example.com -rc 1000
```
📎 **Sends 1000 GET requests** to the specified URL

---

### 2️⃣ POST Request with Inline Payload
```bash
lps --url https://www.example.com -rc 1000 --httpmethod "POST" --payload "Inline Payload"
```
📎 **Sends 1000 POST requests** with a plain text payload

---

### 3️⃣ POST Request with File Payload
```bash
lps --url https://www.example.com -rc 1000 --httpmethod "POST" --payload "Path:C:\Users\User\Desktop\LPS\urnice.json"
```
📎 **Sends 1000 POST requests** using a JSON file as payload

---

### 4️⃣ POST Request with Payload URL
```bash
lps --url https://www.example.com -rc 1000 --httpmethod "POST" --payload "URL:https://www.example.com/payload"
```
📎 **Sends 1000 POST requests** where the payload is fetched from a URL

---

# 🌐 Distributed Load Testing

🌍 Distributed testing is supported starting from **v2.0.2_preview**.

📖 Learn more in the [Distributed Load Testing Article](https://github.com/mohaidr/lps-docs/blob/main/articles/9.DistributedLoadTesting.md)

---

# 📚 LPS Documentation

Explore full docs in the [📖 LPS Docs Repo](https://github.com/mohaidr/lps-docs/tree/main)

### Key Sections:
- 🧾 [Commands](https://github.com/mohaidr/lps-docs/blob/main/articles/1.Commands.md)
- 📄 [Articles](https://github.com/mohaidr/lps-docs/tree/main/articles)
- 🧠 [Concepts](https://github.com/mohaidr/lps-docs/tree/main/concepts)
- 💡 [Examples](https://github.com/mohaidr/lps-docs/tree/main/examples)
- 🔄 [Migration Guide](https://github.com/mohaidr/lps-docs/blob/main/articles/10.MigrationGuide.md) - Upgrading from v3.0.2.5 to v3.0.2.6+


# 🚨 Important Notice

> **⚠️ Warning:** This documentation applies **only** to **version 2.0_Preview** and above of the **LPS Tool**.
> For earlier versions, please visit the [`readme.md`](https://github.com/mohaidr/lps-tool/tree/main/Version) in each version's directory.
> 
> ⚠️ **Note:** Version **2.x is NOT backward compatible** with 1.x.

> **🔄 Breaking Change in v3.0.2.6:** The `failureCriteria` and old `terminationRules` formats have been **permanently removed**. The last version supporting the old format was **v3.0.2.5**. See the [Migration Guide](https://github.com/mohaidr/lps-docs/blob/main/articles/10.MigrationGuide.md) for details on the new `failureRules` and `terminationRules` inline metric expression format.

---
