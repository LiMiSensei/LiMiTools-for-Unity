# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).




## [1.0.0] - 2025-09-28

### Added
* New `activated` property in NavMeshLink, useful to control whether agents are allowed to traverse the link
* New `occupied` property in NavMeshLink, useful to determine whether an agent is using the link
* New `startTransform`, `endTransform` properties in NavMeshLink, useful to define the ends of the link through Transform references as an alternative to points in 3D space
* New `autoUpdatePositions`, `biDirectional`, `costOverride` properties and the `UpdatePositions()` method, introduced as "deprecated" in order to facilitate the upgrade from OffMeshLinks

### Changed
* The minimum supported editor version is Unity 6 LTS. Compatibility with Unity 2023.2 is no longer guaranteed going forward.

### Fixed
* Baking NavMeshSurface no longer fails when the game object name has invalid file name characters.

### Removed
* The "Navigation (Obsolete)" window has been removed. This in turn removes the deprecated abilities to enable the "Navigation Static" flag on scene objects and to bake a single NavMesh embedded in the scene.