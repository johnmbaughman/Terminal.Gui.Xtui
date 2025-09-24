# Using Documentation Offline

This guide explains how to access Terminal.Gui.Xaml documentation offline for development environments without internet access.

> **Introduced in**: Terminal.Gui.Xaml v1.0 (September 2025)  
> **Last Updated**: September 2025  
> **Status**: Stable

## Why Use Offline Documentation?

- **Air-gapped environments**: Work in secure or isolated networks
- **Reliability**: No dependency on GitHub Pages availability
- **Performance**: Faster access than web browsing
- **Customization**: Modify documentation for internal use
- **Version control**: Match docs to specific code versions

## Methods

### Method 1: Clone and Build (Recommended)

Best for developers who want the latest documentation and can run DocFX.

#### Prerequisites
- Git installed
- .NET 8+ SDK 
- DocFX CLI tool

#### Setup Steps

1. **Clone the repository**:
```bash
git clone https://github.com/johnmbaughman/Terminal.Gui.Xaml.git
cd Terminal.Gui.Xaml
```

2. **Install DocFX** (if not already installed):
```bash
dotnet tool install -g docfx
```

3. **Build the documentation**:
```bash
cd docs
docfx docfx.json
```

4. **Serve locally**:
```bash
docfx docfx.json --serve --port 8080
```

5. **Access documentation**:
   - Open browser to `http://localhost:8080`
   - Documentation includes all API references and guides

#### Updating Documentation

```bash
# Pull latest changes
git pull origin main

# Rebuild documentation  
cd docs
docfx docfx.json

# Serve updated version
docfx docfx.json --serve --port 8080
```

### Method 2: Static Build (Portable)

Best for environments where DocFX cannot be installed or run.

#### Build Static Files

```bash
# Clone and build (one-time setup)
git clone https://github.com/johnmbaughman/Terminal.Gui.Xaml.git
cd Terminal.Gui.Xaml/docs
docfx docfx.json

# Copy built files
cp -r _site/ /path/to/offline-docs/
```

#### Use Static Files

1. **Copy the `_site` folder** to your target environment
2. **Open `index.html`** in any web browser
3. **Navigate normally** - all links are relative and work offline

### Method 3: Download Release Package

> 💡 **Coming Soon**: Pre-built documentation packages will be available in GitHub Releases starting with v1.1.

## Customization

### Branding and Themes

Modify the appearance for internal use:

1. **Edit templates**:
   - Templates: `docs/templates/`
   - Styles: `docs/templates/styles/`

2. **Customize metadata**:
   - Site info: `docs/docfx.json`
   - Navigation: `docs/toc.yml`

3. **Add custom content**:
   - Internal guides: `docs/articles/internal/`
   - Company-specific examples: `docs/articles/examples/internal/`

### Version Pinning

Lock documentation to specific framework versions:

```bash
# Checkout specific version
git checkout v1.0.0

# Build documentation for that version
cd docs
docfx docfx.json

# Archive the build
tar -czf terminal-gui-xaml-docs-v1.0.0.tar.gz _site/
```

## Troubleshooting

### Common Issues

**DocFX build errors**
- Ensure .NET 8+ SDK is installed: `dotnet --version`
- Install DocFX globally: `dotnet tool install -g docfx`
- Check for missing dependencies: `docfx docfx.json --logLevel Verbose`

**Missing API documentation**
- Build the project first: `dotnet build` (from repository root)
- Ensure XML documentation is generated: Check `*.csproj` files for `<GenerateDocumentationFile>true</GenerateDocumentationFile>`

**Broken links in offline mode**
- All links should be relative in the built `_site` folder
- If external links don't work offline, that's expected behavior
- File a bug if internal cross-references are broken

**Port conflicts when serving**
- Change port: `docfx docfx.json --serve --port 9000`
- Or serve from different directory using any HTTP server

### Performance Optimization

**Large repositories**
```bash
# Shallow clone to reduce download size
git clone --depth 1 https://github.com/johnmbaughman/Terminal.Gui.Xaml.git

# Build only docs (skip tests)
cd docs
docfx docfx.json --logLevel Warning
```

**Incremental builds**
```bash
# Only rebuild changed files
docfx docfx.json --incremental
```

## Distribution Options

### Internal Networks

1. **Shared network drive**: Copy `_site` folder to shared location
2. **Internal web server**: Host the static files on intranet
3. **Container deployment**: Package in Docker/Podman container
4. **Archive distribution**: Create ZIP/TAR files for easy sharing

### Container Example

```dockerfile
# Dockerfile
FROM nginx:alpine
COPY _site/ /usr/share/nginx/html/
EXPOSE 80
```

```bash
# Build and run
docker build -t terminal-gui-xaml-docs .
docker run -p 8080:80 terminal-gui-xaml-docs
```

## File Structure

The built documentation contains:

```
_site/
├── index.html              # Main documentation page
├── api/                    # API reference
├── articles/               # Guides and examples  
├── styles/                 # CSS and styling
├── scripts/                # JavaScript functionality
├── search-stopwords.json   # Search configuration
├── toc.html               # Navigation structure
└── manifest.json          # Site metadata
```

All files are static and work without server-side processing.

## See Also

- **[Contributing Guide](index.md)** - General contribution instructions
- **[Docs Style Guide](docs-style-guide.md)** - Documentation standards
- **[DocFX Documentation](https://dotnet.github.io/docfx/)** - DocFX configuration and usage
