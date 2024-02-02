# Team Agreement

### Note: This document is subject to change.

## Meetings & Communication
There will be one mandatory in-person group meeting preceding/following scrum sessions with the TA in the lab to go over plans for the upcoming week. A supplementary meeting will be conducted later in the week if necessary, conducted either online or in-person.

The team will primarily communicate via Discord. Team members are to be available at an appropriate but moderate level. Team members are to be consistent in notifying the group as a whole about progress and/or issues.

## Repository Practices (Branches, Pull Requests, etc.)

The Git repository will be populated with branches that follow the chore/feature design pattern.
When new features are to be developed, an isolated branch is to be created with the naming scheme of “feature/<feature-name>”
When working on bug fixes or doing large refactors, an isolated branch is to be created with the naming scheme of “chore/<task-name>”

When publishing commits to the repository, team members are to write comprehensive messages to accompany the commit, outlining all changes/additions that were made. 

When a pull request is generated for a feature branch, at least two team members must be assigned to perform code review.
The author(s) of the feature branch changes must complete an ["Author Pull Request Checklist"](https://github.com/Bawnorton/Bamboozlers/blob/main/Team%20Management/Author_PR_Template.md) 
and any reviewers must complete a ["Reviewer Pull Request Checklist"](https://github.com/Bawnorton/Bamboozlers/blob/main/Team%20Management/Reviewer_PR_Template.md).

## Policy on Testing

Testing will be done at each step of the development process to make sure that all work on the project will be functional and synergistic with the rest of the codebase. Several methods of testing are to be implemented at each stage, whether it is simply during development of single features, to the merging of several features into the main codebase. The testing strategies that will be implemented are as follows:
* Unit Testing: Before making a commit, successful unit tests of new features should be implemented. After features are merged into the main branch, unit tests should be conducted as part of integration testing to ensure that features work together.
* Regression Testing: Before generating pull requests, up-to-date features from the main branch should be fetched to ensure that features work as intended with the code base. 
* Functional Testing: As part of regression testing, before merging a pull request into the main branch, features to be implemented should be tested by code reviewers (and the author) to ensure that all additions/changes function as intended.
* Integration Testing: When several features are pulled to the main branch, the interaction between them is investigated via the outlined testing methods to ensure that the features work together as intended.
* Acceptance Testing: Done via internal review by teammates of features regularly, and via meetings with the TA to ensure that the expectations for the development project are met.
  * Includes the review of aesthetic (UI) additions/changes as well as the review of general functionality and feature integration.
  * System testing will be conducted as part of Acceptance testing.
* Performance Testing: Not a priority, but the performance of features in development should be tested before being pulled to the main code base to ensure that the performance of the software as a whole is not significantly diminished. Performance testing should also be done as part of integration testing.
  * After several features are merged into the main branch and marked as completed, the team as a whole should do performance testing to investigate any possible issues that may have arisen as part of the integration of features.

