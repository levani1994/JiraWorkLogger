# Jira Work Logger

A Blazor Server application that syncs time tracking data from Scrinio to Jira worklogs.

## Features

- 🎨 Beautiful, modern UI
- ⚡ Real-time preview of time entries
- 🔄 Automatic synchronization from Scrinio to Jira
- 📊 Easy-to-use interface

## Configuration

The application requires the following configuration:

- **Scrinio Token**: Get it from [https://scrin.io/account](https://scrin.io/account)
- **Jira Email**: Your Jira account email
- **Jira API Token**: Get it from [https://id.atlassian.com/manage-profile/security/api-tokens](https://id.atlassian.com/manage-profile/security/api-tokens)
- **Jira Company URL**: Your Jira instance URL (e.g., `https://yourcompany.atlassian.net`)

## Local Development

```bash
dotnet restore
dotnet run
```

## Docker

Build the Docker image:
```bash
docker build -t jira-work-logger .
```

Run the container:
```bash
docker run -p 8080:8080 jira-work-logger
```

## Deployment to Render.com

1. Push your code to a Git repository (GitHub, GitLab, or Bitbucket)
2. Connect your repository to Render.com
3. Create a new Web Service
4. Select "Docker" as the environment
5. Render will automatically detect the `Dockerfile` and `render.yaml`
6. Add any environment variables if needed
7. Deploy!

The application will be available at the URL provided by Render.com.

## Environment Variables

You can configure the application using environment variables or `appsettings.json`:

- `Scrin:Token` - Scrinio API token
- `Jira:Email` - Jira account email
- `Jira:ApiToken` - Jira API token
- `Jira:BaseUrl` - Jira instance URL
- `Settings:TimeZoneOffsetMinutes` - Timezone offset (default: 240)

## License

MIT

