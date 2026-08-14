# Changelog

All notable changes to FocusPocus are documented here.

## [Unreleased]

### Added

- Optimized animated usage preview in the project documentation.
- Restore-defaults action in the behavior settings.
- Checked tray-menu controls for spotlight, clicks, and keystrokes.
- Compact click-only indicator when the spotlight is disabled.

### Fixed

- Settings now stay synchronized between the WinUI interface, tray menu, engine, and global shortcuts.
- Settings writes are atomic to prevent partial reads while both processes are running.
- Upgrades no longer reuse the legacy installation directory name.
- The dimming layer is forcibly collapsed during click-only mode, including after spotlight fade animations.

## [2.1.0] - 2026-08-13

### Added

- Built-in GitHub Releases update checker.
- Public project documentation, preview gallery, CI, and release automation.

### Changed

- Migrated the settings interface to native WinUI 3.
- Refined responsive layout, bilingual text, brand presentation, and Windows 11 styling.
- Windows startup now launches only the tray engine.

### Existing capabilities

- Spotlight size up to 800 px with 10% shortcut steps.
- Overlay opacity shortcuts and a native color picker.
- Click and keystroke visualization, including shortcuts-only mode.
- Smooth multi-monitor overlay transitions.
