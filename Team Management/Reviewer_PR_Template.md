## Code Review Checklist

### Reviewer: [Reviewer's Name]
* * *
### 1. Code Correctness and Efficiency
- [ ] Does the code do what it is supposed to do?
- [ ] Are there any potential bugs or logic errors?
- [ ] Does the code have any real, noticeable impacts on performance?
- [ ] Are errors and exceptions handled appropriately? (Fail fast and early if there is some incorrect state, i.e throw your own exceptions when something is wrong instead of waiting for that wrong thing, like a null-ptr to be de-referenced)

### 2. Readability and Maintainability
- [ ] Is the code well-organized and easy to understand?
- [ ] Don’t over-abstract
- [ ] Avoid side-effects
- [ ] Are variable, method, and class names descriptive and consistent?
- [ ] Verbosity is fine
- [ ] Is the code free from commented-out code blocks?
- [ ] Do any and all modifications made to the project adhere to the outlined project structure?

### 3. Coding Standards and Best Practices
- [ ] Does the code adhere to standard language conventions?
- [ ] Are there any best practices that should be followed but are not?

### 4. Documentation and Comments
- [ ] Is new functionality documented well in the code and external documentation?
- [ ] Do comments explain "why" something is done and not just "what" is done?

### 5. Testing
- [ ] Are there unit tests for the new code?
- [ ] Code needs to be structured in a way that it is exposed to tests if something is “inaccessible”, the code needs re-structuring
- [ ] Do existing tests need to be updated?
- [ ] Does the new code introduce any potential issues in the existing tests?

### 8. Performance (If applicable)
- [ ] Does this grossly affect performance and bring the app to a halt?

### 9. UI (If applicable)
- [ ] Is the use of the UI intuitive?
- [ ] Is the UI aesthetically pleasing?
- [ ] Is the user able to cause an undesired UI state?
- [ ] Is the UI functional for all modifications/additions?
- [ ] Is the UI accessible to all users?

## Commentary
Specific Feedback

*

Positives:

*

Improvements Suggested:

*

## Approval Status
- [ ] Approved
- [ ] Approved with suggestions
- [ ] Needs further work

