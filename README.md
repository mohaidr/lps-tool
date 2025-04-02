> **Warning:** This documentation is applicable starting from version 2.0_Preview of the LPS tool. For documentation on earlier versions, please refer to the `readme.md` files located within each version's directory on our [repository](https://github.com/mohaidr/lps-tool/tree/main/Version). Please note that 2.x is not backward compatible with 1.x.

# Introduction

The **LPS Tool** (Load, Performance, and Stress Testing Command Tool) is a versatile and powerful framework designed for evaluating the performance and resilience of web applications under simulated load conditions. By leveraging HTTP-based tests, the tool enables users to assess system behavior, scalability, and endurance with precision.

Built around the innovative concept of **Rounds** and **Iterations**, the LPS Tool introduces highly customizable **Iteration Modes** to replicate real-world user interaction patterns and varying traffic loads. With its intuitive configuration, robust feature set, and focus on flexibility, the tool empowers developers and QA engineers to design, execute, and analyze complex testing scenarios effortlessly.

The LPS Tool is your all-in-one solution for ensuring application reliability and performance under diverse and challenging conditions.

### Installation 
- Download the latest version of the LPS tool from the [versions](https://github.com/mohaidr/lps-tool/tree/main/Version) directory.
- Save it in your desired directory.
- Install [ASP.NET Core Runtime 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- On windows machines, Open a terminal as administrator and run the env.ps1 file.
    - If script execution is restricted, you might need to use the command `Set-ExecutionPolicy Unrestricted` to enable it.
    - To restore the original settings, use the command `Set-ExecutionPolicy Restricted`.
- You can now utilize the lps command from any directory.

## Quick Test Example
### Simple GET Request
```bash
lps --url https://www.example.com -rc 1000
```
**Description**: Sends 1000 GET requests to the specified URL.

### POST Request with Inline Payload
```bash
lps --url https://www.example.com -rc 1000 --httpmethod "POST" --payload "Inline Payload"
```
**Description**: Sends 1000 POST requests with an inline payload.

### POST Request with File Payload
```bash
lps --url https://www.example.com -rc 1000 --httpmethod "POST" --payload "Path:C:\Users\User\Desktop\LPS\urnice.json"
```
**Description**: Sends 1000 POST requests with a payload from a file.

### POST Request with Payload URL
```bash
lps --url https://www.example.com -rc 1000 --httpmethod "POST" --payload "URL:https://www.example.com/payload"
```
**Description**: Sends 1000 POST requests with a payload fetched from a URL.

## Distributed Load Testing
The distributed load testing is supported starting from 2.0.2_preview version. Check the article from [here](https://github.com/mohaidr/lps-docs/blob/main/articles/9.DistributedLoadTesting.md)

---


## LPS Docs
 ### The LPS Docs can be fond from [here](https://github.com/mohaidr/lps-docs/tree/main)

   - [Commands](https://github.com/mohaidr/lps-docs/blob/main/articles/1.Commands.md)
   - [Articles](https://github.com/mohaidr/lps-docs/tree/main/articles)
   - [Concepts](https://github.com/mohaidr/lps-docs/tree/main/concepts)
   - [Examples](https://github.com/mohaidr/lps-docs/tree/main/examples)
