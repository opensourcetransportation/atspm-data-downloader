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
2. Via the Environment Variable `DownloaderConfiguration__ApiKey`
3. Via the `"ApiKey"` setting in your `appsettings.json` file

---

## ⚙️ Configuration & Options

The application dynamically merges and resolves configuration settings with the following priority order:
**Command Line Flag (Highest Priority) > Environment Variable > Application Settings (Lowest Priority)**

### Options Map

| Parameter | Command CLI Option | Environment Variable | Default Value | Description |
| :--- | :--- | :--- | :--- | :--- |
| **Start Date** | `-s`, `--start` | `DownloaderConfiguration__Start` | *Required* | Inclusive download start date/time (e.g., `yyyy-MM-dd` or `yyyy-MM-ddTHH:mm:ss`) |
| **End Date** | `-e`, `--end` | `DownloaderConfiguration__End` | *Required* | Inclusive download end date/time (e.g., `yyyy-MM-dd` or `yyyy-MM-ddTHH:mm:ss`) |
| **Location IDs** | `-l`, `--location` | `DownloaderConfiguration__LocationIdentifiers__0` | *Required* | One or more space-separated ATSPM controller location identifiers (e.g. `1014 1015` or `-l 1014 -l 1015`). For environment arrays, use indices: `LocationIdentifiers__0`, `LocationIdentifiers__1`, etc. |
| **Data Type** | `-t`, `--data-type` | `DownloaderConfiguration__DataType` | *Optional* | Specialized stream category type (e.g., `IndianaEvent`, `ApproachPcdAggregation`) |
| **API Key** | `-k`, `--api-key` | `DownloaderConfiguration__ApiKey` | *Optional* | The authentication API key provided by the ATSPM instance |
| **API URL** | `-u`, `--api-url` | `DownloaderConfiguration__ApiUrl` | *Optional* | Base URL of the API (e.g., `https://your-atspm-instance.gov/data`) |
| **Format** | `-f`, `--format` | `DownloaderConfiguration__Format` | `ndjson` | The output format: `ndjson` (highly recommended), `csv`, or `json` |
| **Output Path** | `-o`, `--output` | `DownloaderConfiguration__OutputPath` | *Current Dir* | Destination directory path where downloaded files are saved. Files are saved as `{location}-{dataset}-{start}-{end}.{ext}` inside this directory. |

---

## 📄 Configuration File (`appsettings.json`)

You can persist your common configurations (such as endpoint URLs and API keys) inside your local `appsettings.json` file. The file is structured as follows:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Warning",
      "atspm_data_downloader": "Debug"
    }
  },
  "DownloaderConfiguration": {
    "Start": "2026-07-24T00:00:00",
    "End": "2026-07-25T00:00:00",
    "LocationIdentifiers": [
      "1014",
      "1015",
      "2023"
    ],
    "DataType": null,
    "ApiKey": "YOUR_API_KEY_HERE",
    "ApiUrl": "https://your-atspm-instance.gov/data/",
    "Format": "ndjson",
    "OutputPath": "output/"
  }
}
```

---

## 🚀 Running locally (CLI Examples)

Below are some common real-world execution examples when running directly via the `.NET CLI`:

### 1. Download Raw Event Logs
NDJSON (Newline-Delimited JSON) is the recommended format for streaming very large datasets. It allows records to be read line-by-line downstream with a minimal memory footprint.
```bash
dotnet run -- download events \
  --location 1014 \
  --start 2026-07-24 \
  --end 2026-07-25 \
  --format ndjson \
  --output ./output
```

### 2. Download Aggregation Logs to CSV
Downloads approach aggregations for a controller and structures them directly into a tabular CSV file.
```bash
dotnet run -- download aggregations \
  --location 1014 \
  --start 2026-07-24 \
  --end 2026-07-25 \
  --format csv \
  --output ./output
```

### 3. Bulk Downloads using an External Location File (`@locations.rsp`)
The CLI natively supports parsing arguments from an external "response file" using the `@` symbol prefix. This allows you to manage lists of controller identifiers inside a flat text file without cluttering the main command line.

Create a file named `locations.rsp` containing the target signal location CLI arguments (one per line):

**locations.rsp:**
```text
--location
1014
1015
2023
2024
```

Then, execute the downloader by referencing the `.rsp` file directly:
```bash
dotnet run -- download events @locations.rsp \
  --start 2026-07-24 \
  --end 2026-07-25 \
  --format ndjson \
  --output ./output
```
This is fully cross-platform and works seamlessly inside Command Prompt, PowerShell, Bash, and Docker.

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
  download events -l 1014 -s 2026-07-24 -e 2026-07-25 -k YOUR_API_KEY_HERE -u https://your-atspm-instance.gov/data -o /app/output
```

#### Aggregations Download:
```bash
docker run --rm \
  -v ${PWD}/output:/app/output \
  ghcr.io/opensourcetransportation/atspm-data-downloader:latest \
  download aggregations -l 1014 -s 2026-07-24 -e 2026-07-25 -k YOUR_API_KEY_HERE -u https://your-atspm-instance.gov/data -o /app/output
```

---

### Docker Compose Multi-Scenarios

To run containerized bulk downloads, create a **`docker-compose.yml`** file inside the respective task folder. Place your `locations.rsp` file in that same folder. 

#### Local `.env` Template:
```ini
DownloaderConfiguration__ApiKey=YOUR_API_KEY_HERE
DownloaderConfiguration__ApiUrl=https://your-atspm-instance.gov/data
DownloaderConfiguration__Start=2026-07-24
DownloaderConfiguration__End=2026-07-25
```

#### Scenario A: Download Raw Event Logs (`docker-compose.yml`)
```yaml
services:
  atspm_event_downloader:
    image: ghcr.io/opensourcetransportation/atspm-data-downloader:latest
    container_name: atspm_event_downloader
    environment:
      - DownloaderConfiguration__ApiKey=${DownloaderConfiguration__ApiKey}
      - DownloaderConfiguration__ApiUrl=${DownloaderConfiguration__ApiUrl}
      - DownloaderConfiguration__Start=${DownloaderConfiguration__Start}
      - DownloaderConfiguration__End=${DownloaderConfiguration__End}
      - DownloaderConfiguration__Format=ndjson
      - DownloaderConfiguration__OutputPath=/app/output
    volumes:
      - ./output:/app/output
      - ./locations.rsp:/app/locations.rsp
    command: ["download", "events", "@/app/locations.rsp"]
```

To execute, run inside this folder:
```bash
docker compose run --rm atspm_event_downloader
```

#### Scenario B: Download Approach Aggregations (`docker-compose.yml`)
```yaml
services:
  atspm_aggregation_downloader:
    image: ghcr.io/opensourcetransportation/atspm-data-downloader:latest
    container_name: atspm_aggregation_downloader
    environment:
      - DownloaderConfiguration__ApiKey=${DownloaderConfiguration__ApiKey}
      - DownloaderConfiguration__ApiUrl=${DownloaderConfiguration__ApiUrl}
      - DownloaderConfiguration__Start=${DownloaderConfiguration__Start}
      - DownloaderConfiguration__End=${DownloaderConfiguration__End}
      - DownloaderConfiguration__Format=csv
      - DownloaderConfiguration__OutputPath=/app/output
    volumes:
      - ./output:/app/output
      - ./locations.rsp:/app/locations.rsp
    command: ["download", "aggregations", "@/app/locations.rsp"]
```

To execute, run inside this folder:
```bash
docker compose run --rm atspm_aggregation_downloader
```

---

*Note: If you want to explicitly supply the file path via the `-f` flag in either scenario, the correct syntax is `-f docker-compose.yml`:*
```bash
docker compose -f docker-compose.yml run --rm atspm_event_downloader
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
