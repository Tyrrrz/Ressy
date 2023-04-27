# Changelog

## v1.0.3 (27-Apr-2023)

- Fixed NuGet packaging issues.

## v1.0.2 (27-Apr-2023)

- Improved support for older target frameworks via polyfills.

## v1.0.1 (21-Jun-2022)

- Fixed an issue where calling `SetIcon(...)` (and potentially other methods that update resources) sometimes resulted in a `Win32Exception` with the message `Parameter is incorrect`.