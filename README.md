# DevToys-DuplicateDetector

DuplicateDetector is an extension for [DevToys](https://devtoys.app/) that finds duplicates in the lines of provided text.

## Installation

After installing DevToys, click on **Manage extensions** then on **Find more extensions online** or go to this page: [nuget.org](https://www.nuget.org/packages?q=Tags%3A%22devtoys-app%22).

A filtered list of all extensions for DevToys should appear. Locate *DuplicateDetectorExtension* and download the NuGet package.

Then, in DevToys, click on **Install extension** and select the NuGet package you downloaded.

The *DuplicateDetectorExtension* should be available after restarting DevToys.

## Usage

This extension is quite straightforward. Just paste the text in the **Input zone** and the found duplicates are shown in the **Duplicate zone**. The lines of each duplicate are shown between brackets.
Additionally, the duplicates are highlighted in the input text.

DuplicateDetector has two modes: `Line` and `Offset/Length`.

### Line Mode

This mode searches for duplicates on whole lines.
![LineMode](doc/dde-doc-mode-line.png)

### Offset/Length Mode

This mode allows you to configure a part of the line (with a zero-based offset and a length) to search for duplicates. In this mode, the searched part of the lines is highlighted in green, while the found duplicates are still highlighted in red.
![OffsetLengthMode](doc/dde-doc-mode-offsetlength.png)
