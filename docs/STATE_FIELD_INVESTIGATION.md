# Field Investigation: State Dropdown Containing "India"

**Target Page:** `https://app.cloudqa.io/home/AutomationPracticeForm`  
**Investigation Date:** November 10, 2026  
**Auditor:** Senior SDET & QA Automation Specialist  

---

## 1. Problem Statement

During previous execution of the test suite against `https://app.cloudqa.io/home/AutomationPracticeForm`, the framework reported:
```
[PASS] State | Conf: 100.0% | Expected: India | Actual: India
```
Because "India" is a sovereign nation and not a state or province, this requires rigorous forensic investigation:
1. Did the framework mistakenly select a "Country" dropdown instead of the "State" dropdown?
2. Does the target page contain separate Country and State fields?
3. What is the actual markup of the target website?

---

## 2. Comprehensive DOM Element Inspection Table

The table below documents every dropdown, select, and geographic input element present on `https://app.cloudqa.io/home/AutomationPracticeForm`:

| Element | Observed label | Available options | Semantic interpretation | Selected by engine |
|---|---|---|---|---|
| `<input id="country" name="Country" type="text" placeholder="Country" class="form-control">` | `<label name="Country" for="country" class="control-label"> Country </label>` | N/A (Autocomplete text input backed by JavaScript array `var countries = ["Afghanistan", ..., "India", ...]`) | **Country input** (Autocomplete Text Field, NOT a `<select>`) | **NO** (Rejected: Technical type is `text`, not `select`) |
| `<select id="state" name="State" class="form-control">` | `<label name="State" for="state" class="control-label"> State </label>` | 34 options:<br>1. `-- Select Country --`<br>2. `Afganistan`<br>3. `Argentina`<br>4. `Australia`<br>...<br>16. `India`<br>...<br>32. `United States`<br>33. `Zimbabwe` | **State field with application-level data anomaly** (The developer labelled the dropdown "State" and gave it `id="state" name="State"`, but populated it with country names) | **YES** (Score: 100%, Matched on associated label `"State"`, tag `select`, and attribute `name="State"`) |
| `<div class="dropdown"><button class="btn btn-primary dropdown-toggle">Know about us</button>...</div>` | `"Know about us"` | Links (`<a>`):<br>- `Application Login`<br>- `Cloudqa Site` | **Navigation Menu Button** (Not a form input) | **NO** (Rejected: Not a `<select>` form control) |
| `<select id="state" name="State">` inside `<shadow-form>` | `<label name="State" for="state"> State </label>` inside shadow root template | Same 34 options as light DOM select | **Shadow DOM practice widget** (Slotted inside custom element `<shadow-form>`) | **NO** (Standard light DOM query targets primary form `#automationtestform`) |

---

## 3. Forensic Findings & Root Cause Analysis

### 3.1 The Target Page Has a Built-in Data Inconsistency
The inspection of `target_page.html` (lines 101–143) provides undeniable proof:
```html
<div class="autocomplete form-group col-xs-6">
    <label name="Country" for="country" class="control-label"> Country </label>
    <input id="country" type="text" name="Country" placeholder="Country" class="form-control" />
</div>
<div class="form-group col-xs-6">
    <label name="State" for="state" class="control-label"> State </label>
    <select class="form-control" id="state" name="State">
        <option> -- Select Country -- </option>
        <option value="Afganistan"> Afganistan </option>
        <option value="Argentina">Argentina</option>
        <option value="Australia">Australia</option>
        ...
        <option value="India">India</option>
        ...
        <option value="United States">United States</option>
        <option value="Zimbabwe">Zimbabwe</option>
    </select>
</div>
```

1. **Country is an `<input type="text">`**, not a dropdown. It uses a custom jQuery autocomplete widget populated with world country names.
2. **State is the only `<select>` element** in the primary form `#automationtestform`.
3. **The target application itself contains an erroneous option list.** The developers created a `<select id="state" name="State">` accompanied by `<label for="state"> State </label>`, but populated it with country names and a placeholder option reading `-- Select Country --`.
4. **The engine DID NOT select the wrong element.** The engine correctly searched for `FieldType.Dropdown` with synonym `"State"`, matched `<label for="state"> State </label>`, and interacted with `<select id="state">`.
5. Because "India" is listed as Option 16 in this exact `<select>` element (`<option value="India">India</option>`), configuring `"India"` was necessary to select a valid option on the live site.

---

## 4. Multi-Dropdown Disambiguation Verification

To guarantee that the framework can distinguish between **Country** and **State** when *both* fields are implemented as `<select>` dropdowns:

1. In [`FieldDefinition.CreateState`](file:///c:/Users/Madugu%20Shiva%20Kumar/OneDrive/图片/Desktop/adaptive-selenium-automation/src/AdaptiveAutomation/Models/FieldDefinition.cs), `"Country"` is explicitly included in `NegativeKeywords`:
   ```csharp
   NegativeKeywords = new[] { "Country", "City", "District", "Zip", "Postal", "County" }
   ```
2. When a `<select>` element is accompanied by a label or attribute matching `"Country"`, `ElementScorer` applies a **-50 point Contradictory Penalty**.
3. In `fixtures/similar-fields.html` and the expanded fixture `fixtures/similar-fields-expanded.html`, both a `Country` `<select>` and a `State` `<select>` exist.
4. Test execution explicitly asserts:
   ```csharp
   Assert.That(stateResult.Candidate!.Id, Is.EqualTo("state_select"));
   ```
   The engine selects the intended State dropdown with high confidence and completely ignores the Country dropdown.

---

## 5. Conclusion & Configuration Decision

- **Field Identity:** The selected element is unequivocally the element designated by CloudQA as the State field.
- **Test Configuration:** `"India"` is configured in `appsettings.json` because it is an actual option present in CloudQA's State `<select>` element.
- **Transparency:** The geographic discrepancy is documented as an external target site inconsistency, not a framework error.

