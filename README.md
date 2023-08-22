# RecruitmentTracking

the main source of this program come from: https://github.com/ignatiusck/RecruitmentTracking

## RecruitmentTracking V2.0

## Requirement Analysis 16/08/23

- [ ] Integrate with google APIs to:
  - [ ] add interview dates to google calendar
  - [ ] add interview links via google meet
  - [ ] add misc file for hiring process to google drive
- [ ] Add job category to ease user navigation
- [ ] Documentation update
- [ ] Unit test the website
- [ ] Construct class diagram
- [ ] Provide before after development

## Proposed first sprint: Job Categories addition

workflow

1. update database via sqlitestudio
2. update EF to handle job category models
3. update backend logic to serve frontend
4. update frontend to display jobs based on the category

proposed categories (+ saran mas kinar)

1. RnD Department
   1. ME
   2. SE
   3. EE
2. HRD
3. Marketing

## Guidelines for development branch workflow

1. Clone my forked repository [here](https://github.com/LuqmanAH/RecruitmentTracingBeta-v2.0.git)
2. type the following command at the root repository:

    ```bash
    git fetch --all
    ```

    check the available branches successfully fetched:

    ```bash
    git branch -a
    ```

    this should be available:

    ```bash
    remotes/origin/batch3Dev
    ```

    checkout and create local branch of that branch:

    ```bash
    git checkout -b batch3Dev origin/batch3Dev
    ```
    from that branch, create a new branch for private experimentation:

    ```bash
    git checkout -b devLuqman
    ```
    when ready, you can create a pull request to batch3Dev to merge your experiment branch
3. Major changes, experimentation, and alteration to the files are to be made **only** in the experimentation branch