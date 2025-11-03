# GitHub Actions Workflows

This directory contains GitHub Actions workflows for CI/CD.

## Workflows

### `ci.yml`
Runs on every push and pull request to validate the code builds and tests pass.

### `build-and-publish.yml`
Builds and publishes NuGet packages to GitHub Packages. Triggers:
- **On tags**: Publishes versioned releases (e.g., `v1.0.0`)
- **On main/master**: Publishes preview builds (e.g., `1.0.0-preview.123`)
- **Manual dispatch**: Allows manual publishing with custom version

## Publishing

### Automatic Publishing

1. **Tag Release**: Create a git tag to publish a versioned release
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```

2. **Main Branch**: Every push to main/master publishes a preview build

### Manual Publishing

Use the GitHub Actions UI to manually trigger a workflow with a custom version.

## Using the Package

After publishing, configure NuGet to use GitHub Packages:

1. Create a Personal Access Token (PAT) with `read:packages` permission
2. Add NuGet source:
   ```bash
   dotnet nuget add source https://nuget.pkg.github.com/YOUR_USERNAME/index.json \
     --name github \
     --username YOUR_USERNAME \
     --password YOUR_PAT \
     --store-password-in-clear-text
   ```
3. Add package reference:
   ```xml
   <PackageReference Include="MessagingDemo" Version="1.0.0" />
   ```

## Notes

- The workflow automatically generates version numbers from tags
- Preview builds use the format: `1.0.0-preview.{run_number}`
- Tagged releases create GitHub Releases automatically
- All packages include symbols for debugging

