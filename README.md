# Adaptive Selenium Automation Framework (.NET 8 & C#)

[![Build & Test Status](https://img.shields.io/badge/build-passing-brightgreen.svg)]()
[![Tests](https://img.shields.io/badge/tests-9%2F9%20passed%20(100%25)-brightgreen.svg)]()
[![.NET Version](https://img.shields.io/badge/.NET-8.0-blue.svg)]()
[![Selenium Version](https://img.shields.io/badge/Selenium-4.26-orange.svg)]()
[![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Cross--Platform-lightgrey.svg)]()

> **Senior SDET & Automation Specialist Internship Assessment — Task 1**  
> A resilient, adaptive web form automation framework that dynamically identifies and tests form fields without relying on brittle, hardcoded IDs, static CSS classes, or positional XPaths.

---

## 1. Problem Statement

Conventional Selenium test suites are notorious for their fragility:
- **Dynamic Front-End Frameworks (React, Blazor, Angular):** Frequently generate dynamic or randomized element IDs (e.g. `id=":r1:"`, `id="input-184920"`).
- **Styling Redesigns:** Migrating CSS frameworks (e.g., Bootstrap to Tailwind) alters class names, breaking CSS locators.
- **Layout & Order Changes:** Adding promotional banners, rearranging form controls, or reorganizing divs invalidates rigid positional XPaths (`//form/div[2]/input`).

When tests break due to non-functional markup changes, teams suffer from high maintenance overhead, flaky test suites, and eroded confidence in automated pipelines.

---

## 2. Project Objective & Core Capabilities

This project implements an **Adaptive Semantic Identification Engine** in C# and Selenium WebDriver that automatically identifies and interacts with three critical fields on the target form:
1. **First Name** — Text input control.
2. **Gender** — Logical radio button group.
3. **State** — Dropdown/select element.

### Key Resilience Capabilities:
- **Attribute Independence:** Operates successfully when IDs, names, and CSS classes are scrambled or removed.
- **Position Independence:** Operates regardless of field placement or reordering.
- **Structural Independence:** Resilient to nested wrapper divs, custom containers, and wrapping `<label>` tags.
- **Disambiguation Intelligence:** Uses negative semantic scoring to distinguish similar fields (e.g., selecting *First Name* while rejecting *Last Name*, *Middle Name*, or *Company Name*).
- **Ambiguity Safeguard:** Rejects ambiguous, competing candidates with close scores instead of making arbitrary, unsafe guesses.
- **Complete Traceability:** Generates structured JSON reports, rich HTML dashboards, and automated failure screenshots.

---

## 3. Technology Stack

| Technology | Role |
|---|---|
| **C# 12 / .NET 8** | Primary programming language and modern managed runtime |
| **Selenium WebDriver 4.26** | Browser automation engine with modern W3C compliance |
| **ChromeDriver / Selenium Manager** | Automated, zero-config browser driver management |
| **NUnit 4.2 & NUnit3TestAdapter** | Automated unit and integration test framework |
| **Microsoft.Extensions.Configuration** | Hierarchical configuration management (`appsettings.json`, env vars, CLI) |
| **Microsoft.Extensions.Logging** | Structured, colorized console and diagnostic logging |
| **System.Text.Json** | High-performance JSON report generation |
| **Git & GitHub** | Source code version control and submission |

---

## 4. Architecture Overview

```mermaid
flowchart TD
    subgraph ConfigLayer["Configuration Layer"]
        CFG["appsettings.json / CLI Overrides"] --> CL["ConfigurationLoader"]
        CL --> AS["AppSettings (Strongly Typed)"]
    end

    subgraph BrowserLayer["Browser Management"]
        BM["BrowserManager"]
        WDF["WebDriverFactory"]
        WD["IWebDriver (ChromeDriver / Selenium Manager)"]
        BM --> WDF --> WD
    end

    subgraph DiscoveryLayer["Adaptive Discovery Engine"]
        DI["DomInspector"]
        CA["ContextAnalyzer"]
        TN["TextNormalizer"]
        SM["SemanticMatcher"]
        EF["ElementFinder"]

        WD --> DI
        DI --> CA
        CA --> TN
        TN --> SM
        SM --> EF
    end

    subgraph ScoringLayer["Candidate Scoring & Ambiguity Safety"]
        SW["ScoringWeights"]
        ES["ElementScorer"]
        CE["ConfidenceEvaluator"]

        EF --> ES
        SW --> ES
        ES --> CE
        CE --> MR["ElementMatchResult"]
    end

    subgraph ActionLayer["Semantic Action Engine"]
        TFA["TextFieldActions"]
        RGA["RadioGroupActions"]
        DDA["DropdownActions"]

        MR --> TFA
        MR --> RGA
        MR --> DDA
    end

    subgraph ReportingLayer["Reporting & Diagnostics"]
        JRG["JsonReportGenerator"]
        HRG["HtmlReportGenerator"]
        SMG["ScreenshotManager"]
    end

    ActionLayer --> ReportingLayer
```

---

## 5. Project Folder Structure

```
adaptive-selenium-automation/
│
├── src/
│   └── AdaptiveAutomation/
│       │
│       ├── Configuration/
│       │   ├── AppSettings.cs              # Strongly typed configuration models
│       │   └── ConfigurationLoader.cs      # Multi-source config loader (JSON, CLI, ENV)
│       │
│       ├── Browser/
│       │   ├── WebDriverFactory.cs         # Chrome/Edge options and driver lifecycle
│       │   └── BrowserManager.cs           # Driver session manager and screenshot hooks
│       │
│       ├── Discovery/
│       │   ├── DomInspector.cs             # Runtime DOM candidate query engine
│       │   ├── ContextAnalyzer.cs          # Semantic signal extractor (labels, ARIA, headings)
│       │   ├── SemanticMatcher.cs          # Natural language token and synonym matcher
│       │   └── ElementFinder.cs            # Orchestrator combining discovery, scoring & evaluation
│       │
│       ├── Scoring/
│       │   ├── ElementScorer.cs            # Deterministic weighted scoring engine
│       │   ├── ScoringWeights.cs           # Configurable heuristic weights
│       │   └── ConfidenceEvaluator.cs      # Threshold evaluation and ambiguity detection
│       │
│       ├── Models/
│       │   ├── ElementCandidate.cs         # Inspected DOM candidate profile
│       │   ├── ElementMatchResult.cs       # Match result, audit breakdown, confidence
│       │   ├── FieldDefinition.cs          # Semantic field definitions and synonyms
│       │   ├── TestResult.cs               # Test execution telemetry
│       │   └── Enums.cs                    # FieldType, ConfidenceLevel, TestStatus
│       │
│       ├── Actions/
│       │   ├── TextFieldActions.cs         # Robust text input interaction & value verification
│       │   ├── RadioGroupActions.cs        # Radio group discovery, selection & state assertion
│       │   └── DropdownActions.cs          # SelectElement interaction & option verification
│       │
│       ├── Tests/
│       │   ├── BaseTest.cs                 # NUnit setup, teardown, screenshot capture
│       │   ├── FirstNameTests.cs           # First Name field verification
│       │   ├── GenderTests.cs              # Gender radio group verification
│       │   ├── StateTests.cs               # State dropdown verification
│       │   └── AdaptiveRegressionTests.cs   # Comprehensive mutation test suite
│       │
│       ├── Reporting/
│       │   ├── JsonReportGenerator.cs      # Machine-readable JSON report generator
│       │   ├── HtmlReportGenerator.cs      # Visual executive HTML dashboard
│       │   └── ScreenshotManager.cs        # Failure screenshot capture
│       │
│       ├── Utilities/
│       │   ├── TextNormalizer.cs           # Tokenizer, case-folding, whitespace normalizer
│       │   └── WaitHelper.cs               # Explicit wait and retry policy helper
│       │
│       ├── Program.cs                      # Standalone CLI entry point
│       ├── appsettings.json                # Runtime configuration
│       └── AdaptiveAutomation.csproj       # Project build specification
│
├── fixtures/                               # Controlled HTML mutation test fixtures
│   ├── baseline.html                       # Structural model of target form
│   ├── changed-attributes.html             # Randomized IDs, names, and CSS classes
│   ├── reordered-fields.html               # State first, Gender second, First Name last
│   ├── changed-structure.html              # Nested divs, wrapping labels, fieldset/legend
│   ├── similar-fields.html                 # Multiple similar inputs, groups, and selects
│   ├── ambiguous-fields.html               # Competing identical fields (ambiguity test)
│   └── missing-fields.html                 # Omitted fields (failure handling test)
│
├── docs/                                   # In-depth architectural & assessment documentation
│   ├── TARGET_PAGE_ANALYSIS.md             # Detailed DOM inspection of CloudQA site
│   ├── ARCHITECTURE.md                     # Architecture specification & component design
│   ├── ADAPTIVE_IDENTIFICATION.md          # Scoring algorithm & matching heuristics
│   ├── TEST_RESULTS.md                     # Execution logs, scores, and benchmark tables
│   ├── APPROACH_AND_LIMITATIONS.md         # Design decisions, edge cases, and future roadmap
│   └── DEMO_SCRIPT.md                      # 5-minute technical interview demonstration script
│
├── artifacts/                              # Generated execution evidence
│   ├── reports/
│   │   ├── latest-report.json              # Machine-readable JSON telemetry
│   │   └── latest-report.html              # Executive HTML dashboard
│   ├── screenshots/failures/               # Automatic failure screenshots
│   └── logs/                               # Execution logs
│
├── .gitignore                              # Git exclusion rules
├── .editorconfig                           # Code formatting rules
├── README.md                               # Framework documentation
└── AdaptiveAutomation.sln                  # Visual Studio solution file
```

---

## 6. Adaptive Identification Algorithm & Scoring Matrix

Each candidate element is scored against field definitions using calibrated weights:

$$\text{Score}(c) = \min\left(100, \max\left(0, \sum W_i \cdot M_i(c) - \sum P_j \cdot C_j(c)\right)\right)$$

| Signal | Weight | Description |
|---|---|---|
| **Exact Associated Label Match** | `40` | Directly associated `<label for="...">` or `<label>` wrapping input |
| **Exact Accessible Name Match** | `35` | `aria-label`, `aria-labelledby`, or `title` match |
| **Contextual Proximity Text** | `25` | Container headings, `<legend>`, or preceding text siblings |
| **Placeholder Match** | `20` | `placeholder` attribute matching semantic synonyms |
| **Technical Type Match** | `15` | Validates element tag and input type (`text`, `radio-group`, `select`) |
| **Container Relevance** | `15` | Enclosing container header corroboration |
| **Attribute Name / ID Match** | `15` | Semantic tokens inside `name` or `id` attributes |
| **Contradictory Keyword Penalty** | `-50` | Conflicting keywords (e.g. `Last`, `Company`, `Middle` when searching `First Name`) |

### Confidence Classification
- **High Confidence (80–100):** Strong multi-signal corroboration. Safe for direct interaction.
- **Medium Confidence (60–79):** Solid semantic evidence. Safe for direct interaction.
- **Low Confidence (< 60):** Insufficient evidence. The engine safely aborts.
- **Ambiguity Gate ($\Delta \le 5$):** If the top two candidates score within 5 points of each other, the engine triggers an **Ambiguity Refusal** to prevent interacting with the wrong field.

---

## 7. Prerequisites & Installation

### Prerequisites:
- Windows 10/11
- Google Chrome browser installed
- Git for Windows
- .NET 8 SDK (local project SDK included in `.dotnet` or system-wide .NET 8)

### Verification:
```powershell
# Verify .NET SDK
.\.dotnet\dotnet.exe --version
# Output: 8.0.427
```

---

## 8. How to Build & Run

### 8.1 Build the Solution
```powershell
.\.dotnet\dotnet.exe build
```

### 8.2 Run Against the Live CloudQA Target Webpage
```powershell
# Run with Headless Mode enabled (recommended for fast CI runs)
.\.dotnet\dotnet.exe run --project src/AdaptiveAutomation/AdaptiveAutomation.csproj -- --url "https://app.cloudqa.io/home/AutomationPracticeForm" --headless true

# Run with Headless Mode disabled (to watch browser interactions)
.\.dotnet\dotnet.exe run --project src/AdaptiveAutomation/AdaptiveAutomation.csproj -- --url "https://app.cloudqa.io/home/AutomationPracticeForm" --headless false
```

### 8.3 Run Against a Modified URL or Local Fixture
The base URL is completely configurable via CLI without changing source code:
```powershell
# Run against a local mutation fixture
.\.dotnet\dotnet.exe run --project src/AdaptiveAutomation/AdaptiveAutomation.csproj -- --url "file:///C:/Users/Madugu Shiva Kumar/OneDrive/图片/Desktop/adaptive-selenium-automation/fixtures/changed-attributes.html" --headless true
```

### 8.4 Run the Automated Test Suite (NUnit)
```powershell
# Run all 9 tests (Unit, Mutation & Component tests)
.\.dotnet\dotnet.exe test --logger "console;verbosity=normal"

# Run only mutation tests
.\.dotnet\dotnet.exe test --filter "FullyQualifiedName~AdaptiveRegressionTests" --logger "console;verbosity=normal"
```

---

## 9. Mutation Testing Scenarios

| Test Case | Scenario Simulated | Key Verification | Result |
|---|---|---|---|
| `Mutation1_ChangedAttributes` | Scrambled IDs (`u_rand_1092`), names (`param_fn_382`), classes (`box-modern`) | Interacts with First Name, Gender, State via semantic text signals | **PASS** |
| `Mutation2_ReorderedFields` | State placed 1st, Gender 2nd, Last Name 3rd, First Name last | Identifies all fields regardless of DOM order | **PASS** |
| `Mutation3_ChangedStructure` | Deeply nested divs, wrapped `<label>`, `<fieldset><legend>`, `aria-label="State"` | Discovers wrapped inputs and ARIA accessible names | **PASS** |
| `Mutation4_SimilarFields` | Company Name, Middle Name, First Name, Last Name; 3 radio groups; 3 dropdowns | Negative keyword penalties prevent false positive matches | **PASS** |
| `Mutation6_AmbiguousFields` | Two identical First Name fields without distinguishing context | Safely reports **Ambiguous** instead of guessing | **PASS** |
| `Mutation7_MissingFields` | Form contains only Email and Comments (target fields absent) | Fails gracefully with clear diagnostic message | **PASS** |

---

## 10. Sample Execution Output

```
======================================================================
   Adaptive Selenium Web Form Automation Engine (.NET 8)
======================================================================
21:47:39 info: AdaptiveAutomation[0] Target URL: https://app.cloudqa.io/home/AutomationPracticeForm
21:47:39 info: AdaptiveAutomation[0] Headless Mode: True
21:47:39 info: AdaptiveAutomation[0] Test Data: FirstName='Shiva', Gender='Male', State='India'
21:47:39 info: AdaptiveAutomation[0] Navigating to URL: https://app.cloudqa.io/home/AutomationPracticeForm
Starting ChromeDriver 156.0.8078.80
21:47:44 info: AdaptiveAutomation[0] Locating field 'First Name' of type TextInput...
21:47:46 info: AdaptiveAutomation[0] Match result for 'First Name': Score=100, Confidence=High, Ambiguous=False
21:47:46 info: AdaptiveAutomation[0] Entered 'Shiva' into candidate <input id='fname' name='First Name' type='text' label='First Name' placeholder='Name'>
21:47:46 info: AdaptiveAutomation[0] Locating field 'Gender' of type RadioGroup...
21:47:47 info: AdaptiveAutomation[0] Match result for 'Gender': Score=100, Confidence=High, Ambiguous=False
21:47:47 info: AdaptiveAutomation[0] Radio option 'Male' selected: SelectedState=True
21:47:47 info: AdaptiveAutomation[0] Locating field 'State' of type Dropdown...
21:47:48 info: AdaptiveAutomation[0] Match result for 'State': Score=100, Confidence=High, Ambiguous=False
21:47:50 info: AdaptiveAutomation[0] Dropdown option selected. Text='India', Value='India'
21:47:50 info: AdaptiveAutomation[0] WebDriver session disposed cleanly.

----------------------------------------------------------------------
                        EXECUTION SUMMARY                             
----------------------------------------------------------------------
[PASS] First Name   | Conf: 100.0% | Expected: Shiva      | Actual: Shiva      (2.41s)
[PASS] Gender       | Conf: 100.0% | Expected: Male       | Actual: Male       (0.74s)
[PASS] State        | Conf: 100.0% | Expected: India      | Actual: India      (2.61s)
----------------------------------------------------------------------
Results: 3/3 Passed.
Reports Generated:
  - JSON: artifacts\reports\latest-report.json
  - HTML: artifacts\reports\latest-report.html
----------------------------------------------------------------------
```

---

## 11. Reports & Diagnostic Artifacts

- **JSON Report:** `artifacts/reports/latest-report.json`
- **HTML Dashboard:** `artifacts/reports/latest-report.html`
- **Failure Screenshots:** `artifacts/screenshots/failures/*.png` (captured automatically on failure)

---

## 12. Limitations & Future Enhancements

### Limitations
1. **Custom Div/Span Dropdowns:** The engine currently targets standard HTML `<select>` elements. Custom widget libraries (e.g. React-Select) built purely out of `<div>` and `<li>` elements require custom click-and-search actions.
2. **Shadow DOM:** Elements nested strictly inside closed or nested Shadow DOM boundaries require explicit shadow root traversal via `GetShadowRoot()`.
3. **Cross-Origin Iframes:** Web security restricts direct cross-origin iframe DOM inspection without explicit context switching.
4. **Completely Unlabelled Controls:** If a page contains a bare input `<input type="text" />` with zero labels, zero placeholders, and zero context, the score correctly falls below the threshold.

### Future Enhancements
- Support for `role="combobox"` custom dropdowns.
- Automated recursive Shadow Root traversal.
- Computer vision / bounding-box proximity scoring as an additional signal layer.
