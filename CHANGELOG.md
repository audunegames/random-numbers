# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.1] - 2026-05-11

### Added

- Added `RandomNumberGenerator` constructor for seeds of type `int?` that defaults to the default seed.
- Added methods to derive seeds by hasing them with a string key.

## [1.0.0] - 2026-02-14

### Added

- Random number generator class to use for seeded random number generation.
- Choosing and sampling random items from enumerables.
- Choosing weighted items from enumerables
- Shuffling enumerables.