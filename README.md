# ATSPM Data Downloader

An optimized, high-performance command-line utility for streaming and downloading large datasets from the **[Automated Traffic Signal Performance Measures (ATSPM)](https://github.com/opensourcetransportation/atspm)** web APIs. 

## Purpose

The primary purpose of this utility is to act as a companion client for the main ATSPM system, enabling users to request and download extremely large telemetry streams—such as raw controller event logs and signal approach aggregations—without overloading server or client memory.

When querying extensive date ranges, standard API clients typically buffer entire JSON arrays into RAM before saving them, which often leads to client-side Out-Of-Memory (OOM) exceptions and browser timeouts. 

This downloader leverages high-speed HTTP stream-processing to stream data sequentially straight from the server to your storage disk in constant memory.

---

## 🔑 Authentication (API Key Required)

To query and download datasets from production ATSPM API endpoints, you **must obtain an API Key** from your ATSPM administrator. 

The API key can be supplied in three ways (in order of priority):
1. Via the CLI flag `-k` or `--api-key`
2. Via the Environment Variable `ATSPM_API_KEY`
3. Via the `"ApiKey"` setting in your `appsettings.json` file

---

## ⚙️ Configuration & Options

The application dynamically merges and resolves configuration settings with the following priority order:
**Command Line Flag (Highest Priority) > Environment Variable > Application Settings (Lowest Priority)**

### Options Map

| Parameter | Command CLI Option | Environment Variable | Default Value | Description |
| :--- | :--- | :--- | :--- | :--- |
| **Start Date** | `-s`, `--start` | `ATSPM_START` | *Required* | Inclusive download start date/time (e.g., `yyyy-MM-dd` or `yyyy-MM-ddTHH:mm:ss`) |
| **End Date** | `-e`, `--end` | `ATSPM_END` | *Required* | Inclusive download end date/time (e.g., `yyyy-MM-dd` or `yyyy-MM-ddTHH:mm:ss`) |
| **Location ID** | `-l`, `--location` | `ATSPM_LOCATION` | *Required* | The ATSPM controller location identifier (e.g., `1014`) |
| **Data Type** | `-t`, `--data-type` | `ATSPM_DATA_TYPE` | *Optional* | Specialized stream category type (e.g., `IndianaEvent`, `ApproachPcdAggregation`) |
| **API Key** | `-k`, `--api-key` | `ATSPM_API_KEY` | *Optional* | The authentication API key provided by the ATSPM instance |
| **API URL** | `-u`, `--api-url` | `ATSPM_API_URL` | *Optional* | Base URL of the API (e.g., `https://your-atspm-instance.gov/data`) |
| **Format** | `-f`, `--format` | `ATSPM_FORMAT` | `ndjson` | The output format: `ndjson` (highly recommended), `csv`, or `json` |
| **Output Path** | `-o`, `--output` | `ATSPM_OUTPUT` | *Stdout* | Destination file path. If omitted, outputs directly to standard output stream |

---

## 🚀 Running locally (CLI Examples)

Below are some common real-world execution examples when running directly via the `.NET CLI`:

### 1. Download Raw Event Logs to an NDJSON File
NDJSON (Newline-Delimited JSON) is the recommended format for streaming very large datasets. It allows records to be read line-by-line downstream with a minimal memory footprint.
```bash
dotnet run -- download events \
  --location 1014 \
  --start 2026-07-24 \
  --end 2026-07-25 \
  --format ndjson \
  --output events_1014.ndjson
```

### 2. Download Aggregation Logs to a CSV File
Downloads approach aggregations for a controller and structures them directly into a tabular CSV file.
```bash
dotnet run -- download aggregations \
  --location 1014 \
  --start 2026-07-24 \
  --end 2026-07-25 \
  --format csv \
  --output aggregations_1014.csv
```

---

## 🐳 Docker Deployment & Containerization

The official Docker image for this utility is hosted on the GitHub Container Registry:
```bash
docker pull ghcr.io/opensourcetransportation/atspm-data-downloader:latest
```

### Docker Run Examples
You can run the utility as a one-off task using arguments passed straight to the container's entrypoint, mounting a local volume to save output files.

#### Event Log Download:
```bash
docker run --rm \
  -v ${PWD}/output:/app/output \
  ghcr.io/opensourcetransportation/atspm-data-downloader:latest \
  download events -l 1014 -s 2026-07-24 -e 2026-07-25 -k YOUR_API_KEY_HERE -u https://your-atspm-instance.gov/data -o /app/output/events_1014.ndjson
```

#### Aggregations Download:
```bash
docker run --rm \
  -v ${PWD}/output:/app/output \
  ghcr.io/opensourcetransportation/atspm-data-downloader:latest \
  download aggregations -l 1014 -s 2026-07-24 -e 2026-07-25 -k YOUR_API_KEY_HERE -u https://your-atspm-instance.gov/data -o /app/output/aggregations_1014.csv
```

---

### Docker Compose Multi-Scenarios

Below are clean Docker Compose scenario templates for both core download services. Create a local `.env` configuration file in the same directory to feed credentials and run parameters safely:

#### Local `.env` Template:
```ini
ATSPM_API_KEY=YOUR_API_KEY_HERE
ATSPM_API_URL=https://your-atspm-instance.gov/data
ATSPM_LOCATION=1014
ATSPM_START=2026-07-24
ATSPM_END=2026-07-25
```

#### Scenario A: Download Raw Event Logs (`docker-compose-events.yml`)
```yaml
version: '3.8'

services:
  event-downloader:
    image: ghcr.io/opensourcetransportation/atspm-data-downloader:latest
    container_name: atspm_event_downloader
    environment:
      - ATSPM_API_KEY=${ATSPM_API_KEY}
      - ATSPM_API_URL=${ATSPM_API_URL}
      - ATSPM_LOCATION=${ATSPM_LOCATION}
      - ATSPM_START=${ATSPM_START}
      - ATSPM_END=${ATSPM_END}
      - ATSPM_FORMAT=ndjson
      - ATSPM_OUTPUT=/app/output/events_${ATSPM_LOCATION}.ndjson
    volumes:
      - ./output:/app/output
    command: ["download", "events"]
```

#### Scenario B: Download Approach Aggregations (`docker-compose-aggregations.yml`)
```yaml
version: '3.8'

services:
  aggregation-downloader:
    image: ghcr.io/opensourcetransportation/atspm-data-downloader:latest
    container_name: atspm_aggregation_downloader
    environment:
      - ATSPM_API_KEY=${ATSPM_API_KEY}
      - ATSPM_API_URL=${ATSPM_API_URL}
      - ATSPM_LOCATION=${ATSPM_LOCATION}
      - ATSPM_START=${ATSPM_START}
      - ATSPM_END=${ATSPM_END}
      - ATSPM_FORMAT=csv
      - ATSPM_OUTPUT=/app/output/aggregations_${ATSPM_LOCATION}.csv
    volumes:
      - ./output:/app/output
    command: ["download", "aggregations"]
```

To run either compose stack:
```bash
# Run events download
docker-compose -f docker-compose-events.yml up

# Run aggregations download
docker-compose -f docker-compose-aggregations.yml up
```

---

## 🕒 Timezone Considerations

> [!IMPORTANT]  
> ATSPM data is highly dependent on controller configuration. API query parameters (`--start` and `--end`) should align with the native timezone of the target signal controllers (typically local time without DST shifts) to avoid data gaps or duplication.

---

## 🤝 Contributing & Development

Contributions are welcome! To set up the downloader locally:

1. Clone the repository:
   ```bash
   git clone https://github.com/opensourcetransportation/atspm-data-downloader.git
   ```
2. Build the solution cleanly:
   ```bash
   dotnet build
   ```
3. Submit a Pull Request with your feature branch!
