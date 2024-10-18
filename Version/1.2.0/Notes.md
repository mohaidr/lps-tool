## Release Notes

### New Features:
- **Data Sent and Received Metrics:** Introduced a new metric that supports tracking the data sent to and received from your endpoint during the test, providing clearer insights into network usage.
- **Maximize Throughput Feature:** Added a feature to optimize and maximize throughput performance.  
  **Note:** Maximizing throughput may result in increased CPU and memory usage.
- **MaxCoolingPeriod and ResumeCoolingAfter Features:** Introduced two new watchdog features to improve system resilience during cooling periods:
  - **MaxCoolingPeriod (`maxcp`, `--maxcoolingperiod`, `--MAXCOOLINGPERIOD <maxcoolingperiod>`):** This feature sets the maximum cooling period in seconds to prevent the system from being stuck in a never-resuming state. After the specified time, cooling will cease regardless of resource conditions.
  - **ResumeCoolingAfter (`rca`, `--resumecoolingafter`, `--RESUMECOOLINGAFTER <resumecoolingafter>`):** This feature allows the system to resume cooling after a defined period (in seconds), even if resources are still above the threshold. This ensures controlled cooling cycles and prevents unnecessary system stress.
- **Customizable Cool Down Time:** **(Breaking Change)** Updated cool down time configuration to accept milliseconds instead of seconds.

### Enhancements:
- **Save Response Logic:** Enhanced the response saving mechanism to store sample responses rather than every individual one. A response is now saved once every 30 seconds to improve efficiency.
- **Download Embedded HTML Resources:** Embedded HTML resources can now be downloaded even when response saving is not enabled. Note that extracted URLs are cached for up to 30 seconds to prevent performance issues that could affect the load test.
- **Throughput Calculation:** Improved the accuracy of throughput metrics and calculations.
- **Metric Name Update:** Renamed the "Connection Metric" to "Throughput Metric" for clarity.
- **Ramp-Up Period Update:** Renamed "Ramp-Up Period" to "Arrival Delay" for improved terminology.

### Bug Fixes:
- **Memory Leak Fixes:**
  1. Disposed of linked and timeout cancellation tokens properly.
  2. Disposed of file streams to prevent memory leakage.
  3. Replaced manual buffer allocation with a buffer pool to reduce memory overhead.
