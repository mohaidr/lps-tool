# LPS Command-Line Tool Documentation

### Overview
The LPS tool is designed for Load, Performance, and Stress testing of web applications. It allows users to design, manage, and execute a variety of HTTP-based tests to assess application performance under simulated load conditions.

The LPS tool is distinctively built around the concept of iteration modes, setting it apart from other testing tools. This design choice emphasizes flexibility and ease of use, allowing users to run diverse testing scenarios by simply specifying a few key parameters. The iteration modes are central to the tool's functionality, offering a robust way to customize how HTTP requests are issued during tests.

### Installation 
- Download the latest version of the LPS tool from the [versions](https://github.com/mohaidr/lps-tool/tree/main/Version) directory.
- Save it in your desired directory.
- Open a terminal as administrator and run the env.ps1 file.
    - If script execution is restricted, you might need to use the command `Set-ExecutionPolicy Unrestricted` to enable it.
    - To restore the original settings, use the command `Set-ExecutionPolicy Restricted`.
- You can now utilize the lps command from any directory.

### What is an Iteration Mode?
Iteration Mode is a fundamental feature of the LPS tool that dictates the pattern and timing of HTTP requests during a test. This mode allows users to precisely model different user interactions and system loads by managing the sequence and behavior of requests. The Iteration Mode specifies how HTTP requests are structured and executed during a test run, allowing you to mimic different user interaction patterns and system loads, it adjusts the sequencing and timing of requests based on predefined behaviors. 

Here's a clear description of each available mode and what they entail:

#### CB (Cooldown-Batchsize)
- **Functionality**: Issues HTTP requests in batches, pausing after each batch during the cooldown period. This mode helps assess how the application handles intermittent high traffic.<br/>
- **Use Case**: Best for simulating user activity in waves, like during promotional events or scheduled exams.
#### CRB (Cooldown-Request-Batchsize)
- **Functionality**: Builds on the CB mode by also capping the total number of requests, distributing them in controlled batches with breaks.<br/>
- **Use Case**: Ideal for precise stress testing during peak traffic times, such as major sales events.
#### D (Duration)
- **Functionality**: Sends requests continuously one at a time for a set duration, maintaining a constant load to test application endurance.<br/>
- **Use Case**: Suitable for evaluating performance stability over long periods, applicable to systems with consistent user engagement.
#### DCB (Duration-Cooldown-Batchsize)
- **Functionality**: Merges sustained and batched request sending with breaks, offering a detailed view of how traffic spikes are handled over time.<br/>
- **Use Case**: Useful for services that need to perform reliably under fluctuating demand, like ticketing portals during launches.
#### R (Request Count)
- **Functionality**: Completes a fixed number of requests sequentially, ensuring the application can handle each request before moving to the next.<br/>
- **Use Case**: Effective for systems that require orderly processing, such as transactional services where actions must follow a specific sequence.

These modes allow testers to closely replicate various real-world user behaviors and system stresses, aiding in the comprehensive evaluation of web applications under diverse conditions.


# Commands

`lps [options]`

This base command initiates a variety of testing scenarios that can be run immediately without the need to save the test configuration. The behavior of the test is determined by the options specified.

### Quick Test Examples
`lps --url https://www.example.com -rc 1000`

`lps --url https://www.example.com -rc 1000 --httpmethod "POST" --payload "Inline Payload"`

`lps --url https://www.example.com -rc 1000 --httpmethod "POST" --payload "Path:C:\Users\User\Desktop\LPS\urnice.json"`

`lps --url https://www.example.com -rc 1000 --httpmethod "POST" --payload "URL:https://www.example.com/payload"`


### Running On Mac and Linux
- Instal .net 8 Runtime for [Mac](https://learn.microsoft.com/en-us/dotnet/core/install/macos) or [Linux](https://learn.microsoft.com/en-us/dotnet/core/install/linux-ubuntu-install?tabs=dotnet8&pivots=os-linux-ubuntu-2404)
- Run the application using the dotnet command: dotnet LPS.dll.
- You can also run the commands using dotnet LPS.dll [options], for example: dotnet LPS.dll run -tn Hello-Test.
- If the dashboard does not open automatically, please open it manually by visiting the following link: http://127.0.0.1:59444 to view your test results.
- You can also create your own dashboard to view the test results. The Metrics API is accessible at http://127.0.0.1:59444/api/metrics or http://[your local IP]:59444/api/metrics.

#### Options:
    -tn, --testname <testname>: Specifies the test name, defaults to "Quick-Test-Plan".
    -nc, --numberOfClients, --numberofclients <numberOfClients>: Sets the number of clients to execute the plan, defaults to 1.
    -rp, --rampupPeriod, --rampupperiod <rampupPeriod>: Time in milliseconds to wait before a new client starts the test plan, defaults to 0.
    -dcc, --delayClientCreation, --delayclientcreation: Delays client creation until needed, defaults to False.
    -rip, --runInParallel, --runinparallel: Executes tests in parallel, defaults to True.
    -u, --url <url>: Target URL for the test (REQUIRED).
    -hm, --httpmethod, --method <method>: HTTP method to use, defaults to GET.
    -h, --header, --Header <header>: Specifies request headers.
    -rn, --runName, --runname <runName>: Designates the name for the 'HTTP Run', defaults to "Quick-Http-Run".
    -im, --iterationMode, --iterationmode <CB|CRB|D|DCB|R>: Defines the iteration mode for the HTTP run, defaults to R (Random).
    -rc, --requestCount, --requestcount <requestCount>: Specifies the number of requests to send.
    -d, --duration, --Duration <duration>: Duration for the test in seconds.
    -cdt, --coolDownTime, --cooldowntime <coolDownTime>: Cooldown period in seconds before sending the next batch.
    -bs, --batchsize <batchsize>: Number of requests per batch.
    -hv, --httpVersion, --httpversion <httpVersion>: HTTP version, defaults to 2.0.
    -sr, --saveResponse, --saveresponse: Option to save HTTP responses, defaults to False.
	-dhtmler, --downloadHtmlEmbeddedResources, --downloadhtmlembeddedresources: Option to download HTML embedded resources, defaults to False. This will only work if the SaveResponse option is set to true. 
    -p, --payload, --Payload <payload>: Request payload, can be a path to a file or inline text.


### Sub Commands
`lps [command] [options]`

If you prefer to prepare a test in advance and have the flexibility to run it later, you can set up a test plan, incorporate HTTP runs into it, and execute it whenever it's convenient. The plan you create will be saved as a JSON file in the directory from which you execute the command, allowing for easy reuse and modification as needed.

  
### Create Plan:

`lps create [options]`

Initializes a new test plan. Essential parameters such as the test's name, the number of simulated clients, and the ramp-up period are defined here. This command sets the groundwork for a load test, specifying how the test environment will mimic user traffic.


#### Options:

    -tn, --testname <testname>: Required. Sets the name of the test.
    -nc, --numberOfClients <numberOfClients>: Required. Defines how many clients will run the test simultaneously.
    -rp, --rampupPeriod <rampupPeriod>: Required. The delay in milliseconds before initiating a new client.
    -dcc, --delayClientCreation: When set, client creation is postponed until necessary. Default is false.
    -rip, --runInParallel: Determines whether tests are run concurrently. Default is true.
    Help (-?, -h): Displays help and usage information.


### Add HTTP Run

`lps add [options]`

This command adds an HTTP run to an existing test plan. It configures key aspects of the HTTP testing scenario, including iteration mode, target URLs, HTTP method, and other critical elements of the HTTP request. This customization is essential for accurately simulating specific user interactions with the web application.

#### Options:

    -tn, --testname <testname>: Required. Specifies the test to which the HTTP run will be added.
    -rn, --runName <runName>: Required. Names the individual HTTP run.
    -im, --iterationMode <CB|CRB|D|DCB|R>: Defines the behavior for repeating HTTP requests.
    -rc, --requestCount <requestCount>: Indicates the total number of requests to send during the run.
    -d, --duration <duration>: Specifies the length of the test in seconds.
    -cdt, --coolDownTime <coolDownTime>: Sets the waiting time in seconds between batches.
    -bs, --batchsize <batchsize>: The number of requests sent in one batch.
    -hm, --httpmethod <method>: Required. Chooses the HTTP method (e.g., GET, POST).
    -hv, --httpVersion <httpVersion>: Specifies the HTTP protocol version. Default is 2.0.
    -u, --url <url>: Required. The target URL for the HTTP requests.
    -sr, --saveResponse: Decides if the HTTP responses should be saved. Default is false.
	-dhtmler, --downloadHtmlEmbeddedResources, --downloadhtmlembeddedresources: Option to download HTML embedded resources, defaults to False. This will only work if the SaveResponse option is set to true. 
    -h, --header <header>: Adds custom headers to the HTTP requests.
    -p, --payload <payload>: Provides the data sent with the request, either from a file or as inline text.

### Run Test  
`lps run [options]`

Executes a prepared test plan according to its specifications. It requires the name of the test to identify and launch the appropriate testing procedure. This command triggers the actual load, performance, or stress testing by sending the configured HTTP traffic to the target application.

#### Options:

    -tn, --testname <testname>: Required. Identifies the test to be executed.
    Help (-?, -h): Shows help and options available for the command.

### Example
    lps create --testname "login-test-plan-01" --numberOfClients 5 --rampupPeriod 1000
    lps add --testname "login-test-plan-01" --runName "LoginTest" --httpmethod GET --url "https://example.com/login" --iterationMode CRB --requestCount 500 --batchsize 20  --coolDownTime 5
    lps run --testname "login-test-plan-01"



# LPS Configuration
    
### Configure Logger
`lps logger [options]`

Sets up logging parameters for the test operations. This command allows customization of log output locations, console logging preferences, and log verbosity. Effective logging is vital for later analysis and troubleshooting of test results.

#### Options:

    -lfp, --logFilePath <logFilePath>: Specifies the file path for logging output.
    -ecl, --enableConsoleLogging: Enables or disables logging to the console.
    -dcel, --disableConsoleErrorLogging: Toggles error logging to the console.
    -dfl, --disableFileLogging: Enables or disables file logging.
    -ll, --loggingLevel <level>: Sets the verbosity of logs.
    -cll, --consoleLoggingLevel <level>: Determines the detail of logs shown on the console.

### Configure HTTP Client
`lps httpclient [options]`

Configures the HTTP client that will be used for sending requests in a test. Adjustments can be made to connection pooling, client timeout settings, and connection limits per server. These settings optimize the HTTP client's performance to suit the test needs and reduce potential bottlenecks.

#### Options:

    -mcps, --maxConnectionsPerServer <maxConnectionsPerServer>: Limits the number of simultaneous connections per server.
    -pclt, --poolConnectionLifeTime <poolConnectionLifeTime>: Defines how long a connection remains in the pool.
    -pcit, --poolConnectionIdelTimeout <poolConnectionIdelTimeout>: The maximum time a connection can stay idle in the pool.
    -cto, --clientTimeout <clientTimeout>: Sets the timeout for HTTP client requests.

### Configure Watchdog
`lps watchdog [options]`

Manages the watchdog mechanism that monitors and controls resource usage during tests. It establishes thresholds for memory and CPU usage that, when exceeded, will pause the test to safeguard the system. It also defines conditions for when a paused test can resume, ensuring that the testing does not overload the system or network.


#### Options:

    -mmm, --maxMemoryMB <maxMemoryMB>: Sets a memory usage limit that pauses the test upon being reached.
    -mcp, --maxCPUPercentage <maxCPUPercentage>: Specifies a CPU usage limit to pause the test.
    -cdmm, --coolDownMemoryMB <coolDownMemoryMB>: Memory limit for resuming a paused test.
    -coolDownCPUPercentage <coolDownCPUPercentage>: CPU usage threshold for test resumption.
    -mcccphn, --maxConcurrentConnectionsCountPerHostName <maxConcurrentConnectionsCountPerHostName>: Limits concurrent connections per host to pause the test.
    -cdcccphn, --coolDownConcurrentConnectionsCountPerHostName <coolDownConcurrentConnectionsCountPerHostName>: Concurrent connection limit for resuming tests.
    -cdrtis, --coolDownRetryTimeInSeconds <coolDownRetryTimeInSeconds>: Interval for checking if the test can be resumed.
    -sm, --suspensionMode <All|Any>: Decides whether to pause the test when all or any thresholds are exceeded.

### Help
**For additional details on command usage and options:**
`lps -?, -h`

This help command will provide comprehensive usage information for the LPS tool or any specific command.

## Test Plans

Test plans and HTTP runs are saved in a JSON file for future use. This file is stored in the directory where the `create` command is executed, allowing for manual updating or creation of test plans if you choose not to use the command-line tool.

	{
		"lpsRuns": [
			{
				"lpsRequestProfile": {
					"id": null,
					"httpMethod": "GET",
					"url": "https://urnice.azure-api.net/echo/resource",
					"payload": null,
					"httpversion": "2.0",
					"httpHeaders": {
						"Hello-Header": "Hello-Header"
					},
					"downloadHtmlEmbeddedResources": false,
					"saveResponse": false,
					"isValid": true,
					"validationErrors": {}
				},
				"id": null,
				"requestCount": null,
				"duration": 15,
				"batchSize": null,
				"coolDownTime": null,
				"name": "Urnice",
				"isValid": true,
				"mode": 4,
				"validationErrors": {}
			},
			{
				"lpsRequestProfile": {
					"id": null,
					"httpMethod": "GET",
					"url": "https://www.example.com",
					"payload": null,
					"httpversion": "2.0",
					"httpHeaders": {
						"Hello-Header": "Hello-Header"
					},
					"downloadHtmlEmbeddedResources": false,
					"saveResponse": false,
					"isValid": true,
					"validationErrors": {}
				},
				"id": null,
				"requestCount": null,
				"duration": 500,
				"batchSize": null,
				"coolDownTime": null,
				"name": "Example",
				"isValid": true,
				"mode": 4,
				"validationErrors": {}
			},
			{
				"lpsRequestProfile": {
					"id": null,
					"httpMethod": "GET",
					"url": "https://www.example.com/resource",
					"payload": null,
					"httpversion": "2.0",
					"httpHeaders": {},
					"downloadHtmlEmbeddedResources": false,
					"saveResponse": false,
					"isValid": true,
					"validationErrors": {}
				},
				"id": null,
				"requestCount": 500,
				"duration": null,
				"batchSize": null,
				"coolDownTime": null,
				"name": "Hello-Run-2",
				"isValid": true,
				"mode": 3,
				"validationErrors": {}
			}
		],
		"id": null,
		"name": "Example-Resource",
		"numberOfClients": 1,
		"rampUpPeriod": 0,
		"delayClientCreationUntilIsNeeded": false,
		"runInParallel": false,
		"isValid": true,
		"validationErrors": {}
	}




## Flexibility and Testing Capabilities:
The LPS tool through the iteration modes provides significant flexibility in how tests are structured, allowing testers to closely mimic a variety of real-world scenarios. By varying the sequence and intensity of load conditions, these modes help identify potential performance bottlenecks and ensure that the application is robust enough to handle different types of user interactions and traffic patterns. This tailored approach is crucial for developing highly reliable and scalable web applications.


## Dashboard

The LPS tool features a dashboard that enables users to effectively monitor and analyze key metrics for their testing endpoints. These metrics include response time, response breakdown, request rate, and connection metrics, offering comprehensive insights for optimal performance monitoring.

![image](https://github.com/user-attachments/assets/3b8ad978-d89d-41b6-afec-dc0e7468ab23)
![image](https://github.com/user-attachments/assets/4d7a43a0-c919-4a73-aebc-fd97277f05df)