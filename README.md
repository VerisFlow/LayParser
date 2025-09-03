# LayParser

![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)
![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)
![Version](https://img.shields.io/badge/version-v1.0.0-blue.svg)

A Windows utility to convert Hamilton Venus deck layout (.lay) files into a structured Markdown file, designed for seamless integration with the [VerisFlow](https://www.verisflow.com) ecosystem.

---

## What is LayParser?

LayParser is a lightweight desktop tool for Windows that bridges the gap between Hamilton's VENUS software and modern lab automation workflows. It reads the complex `.lay` file, extracts all essential labware data—including IDs, positioning, and dimensions—and exports it into a clean, human-readable Markdown file.

## The VerisFlow Workflow

The primary purpose of LayParser is to empower **[VerisFlow](https://www.verisflow.com)**, a platform that generates Hamilton HSL code from natural language commands using the Gemini API.

The workflow is simple:
1.  **Generate Markdown:** Use **LayParser** to convert your `.lay` file into a Markdown file.
2.  **Upload to VerisFlow:** Upload the generated Markdown file to the VerisFlow platform.
3.  **Visualize & Generate:** VerisFlow uses the file to render an accurate virtual deck. This visualization is crucial for enhancing the quality and reliability of the HSL code generated from your natural language descriptions of pipetting processes.

## Key Features

- **Parse Hamilton `.lay` Files:** Accurately reads deck layout files from the Hamilton VENUS software.
- **Extract Labware Data:** Gathers a list of all associated labware files and extracts critical information like Labware ID, grid position, and dimensions.
- **Generate Structured Markdown:** Exports the collected data into a well-organized Markdown file, ready for use in other systems.
- **Simple Windows UI:** An intuitive and easy-to-use graphical interface.

## Demonstration

Watch the video below to see how LayParser is used to render a virtual deck within VerisFlow.

[![VerisFlow Deck Layout Rendering with LayParser](https://img.youtube.com/vi/KOxgiYlpLOk/0.jpg)](https://www.youtube.com/watch?v=KOxgiYlpLOk)

*(Click the image to watch the YouTube video)*

## Getting Started

### Prerequisites

- Windows 10 or later.
- **Microsoft .NET Framework 4.7.2**. (Note: This is often pre-installed on modern versions of Windows 10 and 11).


### Installation

1.  Go to the [**Releases**](https://github.com/VerisFlow/LayParser/releases) page.
2.  Download the latest `LayParser-v1.0.0.zip` file.
3.  Unzip the folder and run `LayParser.exe`.

## Usage

1.  Launch the `LayParser.exe` application.
2.  Load your layout file by either:
    -   Clicking the **"Select Deck Layout File"** button.
    -   Dragging and dropping the `.lay` file directly onto the application window.
3.  The application will instantly display the labware data and **automatically generate** the `.md` file. The new file will be saved in the same folder as your original `.lay` file, using the same name but with a `.md` extension.
4.  Upload the newly created Markdown file to VerisFlow to visualize your deck.

## Contributing

Contributions are welcome! If you'd like to help improve LayParser, please follow these steps:

1.  Fork the repository.
2.  Create a new feature branch from the `dev` branch (`git checkout -b feature/your-amazing-feature dev`).
3.  Make your changes and commit them.
4.  Push your branch and create a Pull Request targeting the `dev` branch.

Please read our [CONTRIBUTING.md](CONTRIBUTING.md) file for more details on our code of conduct and the process for submitting pull requests.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.