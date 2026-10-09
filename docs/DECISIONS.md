# Decision document

## Summary

- **The brief:** a local bike shop says "we need an app for our shop", and the owner isn't available to answer questions.
- **The problem I picked:** the repair side of the shop. I based this on a marine repair shop I worked at for several years as a teenager. Repairs there were tracked in a spreadsheet that only the owner updated. Nothing was badly broken, but it could have been organised much better, with fewer headaches and less of the owner's time.
- **What I'm building:** Bike Shop Service Desk, an internal web app that tracks each repair from check-in to pickup, plus a dashboard of what's late and what's waiting to be picked up.
- **Who uses it:** the owner (sees everything, and can change or reopen collected and cancelled jobs) and staff (check bikes in, do the work, hand them back).
- **What I'm not building:** payments, invoicing, stock and anything customer-facing. The shop most likely has a POS for sales already (#2), so the app just records the POS receipt number when a bike is collected.
- **Stack:** SvelteKit front end, ASP.NET Core 10 API, SQLite, all in one Docker image that starts with one command.

## 1. Assumptions

| # | Assumption | Why | If it's wrong |
| --- | --- | --- | --- |
| 1 | A small single-location shop: the owner plus 1 to 3 staff, who all work both the counter and the workshop. | The brief says "local, independent" and the owner is busy, so it's most likely a small team where everyone does a bit of everything. | More staff or more locations would need separate counter and mechanic roles, and a location on each job. |
| 2 | The shop already uses a POS for sales, card payments and tax, but it doesn't track repairs. | Almost every shop has one for sales. Most don't track repairs, which would explain why the owner asked for an app. | If their POS already tracks repairs, they may not need this app at all, or only the dashboard. |
| 3 | Repairs are tracked today with paper tags on the bikes, a spreadsheet and memory. | That's how the marine repair shop I worked at did it: the owner kept the spreadsheet and updated it all. | If repairs are already tracked in a spreadsheet, this will give a centralised system where they can easily track current repairs and find old ones when new work is needed to be done. |
| 4 | Roughly 10 to 30 repair jobs a week, with a backlog in the spring. | That's about what a team that size can get through at an hour or two per job, and spring is the busy season. The marine shop I worked at had the same spring backlog. | If it's much busier, the job board would need paging and more filters to stay quick to use. |
| 5 | Customers drop bikes off in person. There's no online booking. | Most small shops work walk-in or by phone. | Online booking would just be another way of creating jobs on top of this. |
| 6 | Labour is estimated in hours at check-in. The customer pays the estimated labour plus the parts actually used. If extra work would push the bill past what the customer agreed to, staff call them first. If a mechanic is just slower than estimated, the shop absorbs it. | That's how the marine shop worked: extra work was only done once the customer approved it. It's also what customers expect from a quote. | If the shop bills actual hours instead, the bill is calculated in one place, so it's a small change. |
| 7 | Customers pay when they pick the bike up. | Payment goes through the POS, so a finished bike that hasn't been paid for is just a bike that hasn't been picked up yet. | Business customers who pay on account would need invoicing, which I've left out. |
| 8 | Staff share two desktop PCs, one at the counter and one in the workshop. Everyone has their own account. | The shop probably has these PCs already, and personal accounts show who did what. Labour can be logged against any mechanic, whoever is signed in. | A shared tablet in the workshop would need a touch-friendly layout and quick user switching. |
| 9 | Staff write the job number on the paper tag that goes on the bike. | Something physical on the bike has to point to its record. | Staff would have to find a bike's job by customer name and the bike's description, which is slower. |
| 10 | One time zone (Eastern), prices in Canadian dollars, amounts before HST. | It's a local shop, and the POS adds tax at the till. | Other regions would need time zone and tax settings. |
| 11 | Staff can see job prices and the dashboard (late jobs, bikes waiting to be picked up). | Staff talk prices with customers every day, and in a small shop everyone helps clear late jobs, so the whole team should see them. | If the owner wanted figures only they could see, such as profit, those would need their own API endpoint checked on the server, because the dashboard is built from data staff already have. |

## 2. Problem selection

### Who has what problem

| User | Problem today |
| --- | --- |
| Owner | Is the only one updating the spreadsheet, so keeping it current takes a lot of their time, and it's hard to see at a glance what's late or waiting to be picked up. |
| Staff at the counter | To answer "is my bike ready?" they have to go and check the tags. Promised dates and what the customer agreed to are on paper that gets lost. There's no history for returning customers. |
| Staff in the workshop | Not always clear what to work on next, or what the customer agreed to pay for. |
| Customers | Don't know when their bike will be ready, or whether it'll cost more than they were told. |

### Options I considered

| Option | Value to the shop | Does the POS already cover it? | Reason | Decision |
| --- | --- | --- | --- | --- |
| Repair jobs, from check-in to pickup | High. Labour has no stock cost, so repairs usually make the best margin, and it's where most day-to-day problems are. | Usually not | Booking, notifications and a status page for customers all build on it. | **Chosen** |
| Online store | Medium | Often | It's a separate sales channel and doesn't help with running the shop day to day. | Not now |
| Stock / inventory | Medium | Yes | The POS already handles it. | Not needed |
| Online booking | Medium | Sometimes | It needs a job system to book into first. | Later |
| Bike rentals | Low, and not every shop rents | Rarely | Only useful to some shops, and mostly in the busy season. | Not now |

### Why repair jobs first

- It's the most valuable part of the shop that the POS doesn't already handle.
- Everything else I'd add later (booking, notifications, a status page) needs a job record to exist first.
- Counter staff, mechanics and the owner all work from the same job records. The job board shows mechanics what to do next, and the dashboard shows everyone what's late. When a customer calls to ask if their bike is ready, staff can look it up by phone number.
- The dashboard only works if staff keep their jobs up to date, so checking a bike in and logging time need to be quick.

Customers don't use the app themselves, but it still helps them. Staff get a warning when a bill goes over what the customer agreed to, so they can call and get approval before pickup. A status page for customers and "ready for pickup" messages are next steps.

### How the owner would know it's working

- Fewer "is my bike ready?" calls.
- Fewer bills that go over what the customer agreed to.
- Finished bikes spend less time waiting to be picked up.
- Estimates get closer to the time actually spent.

## 3. Scope

### What I built

- **Sign-in with two roles:** Owner and Staff.
- **Check-in:** find a returning customer by phone or name, or add a new one. Record the bike, the work wanted, the estimate and the promised date. The estimate is labour hours at the job's own hourly rate plus parts, and it's what the customer agrees to.
- **Job board:** every bike in the shop by status, sorted by promised date, with search and an "assigned to me" filter.
- **Job statuses:** Checked in, In progress, Ready for pickup and Collected, plus On hold (waiting for parts or for the customer) and Cancelled. Collecting a bike records the POS receipt number.
- **Work records:** labour time and parts used, a running bill, and a warning when the bill goes over the estimate. If the customer agrees to more work, staff revise the estimate.
- **Notes:** a running log on each job, such as "called, left voicemail". Putting a job on hold from the over-estimate warning adds a note saying what to ask the customer.
- **Closed jobs:** collecting a bike needs the POS receipt number and cancelling a job needs a reason, each confirmed in a dialog, so one stray click can't close a job. Staff can't change collected or cancelled jobs. The owner can correct them, or reopen one closed by mistake.
- **Dashboard:** bikes in the shop, overdue jobs grouped by status, and bikes waiting to be picked up with the money still to be paid.

Everything else I planned is in [next steps](#6-next-steps).

### Not building, and why

| Not building | Why |
| --- | --- |
| Payments, invoicing and tax | The POS already does this. Doing it twice would mean two places to keep in sync. The receipt number links them. |
| Connecting to the POS or accounting software | Useful, but it depends on which systems the shop uses, and the job records need to be in place and trusted first. |
| Stock and ordering parts | That's the POS's job. Parts used are recorded on the job as simple lines. |
| Anything customer-facing | It needs a reliable job system underneath first. |
| A price list of standard services | Estimates are typed in for now. Prefilling them from a list is a good next step. |
| Phone and tablet layouts | Staff use desktop PCs (#8). |

## 4. Technical choices

| Part | Choice | Why |
| --- | --- | --- |
| Front end | SvelteKit (single-page app), TypeScript, Tailwind CSS | I used SvelteKit on a previous project, so I can build quickly with it. A single-page app suits an internal tool that stays open all day. |
| Back end | ASP.NET Core 10 Minimal API in C# | C# is my strongest language, and Minimal APIs keep each endpoint short and easy to follow. |
| Database | SQLite with Entity Framework Core | A few users and not much data. No database server to run, and the whole database is one file. |
| Packaging | One Docker image, started with `docker compose up --build` | One command to run it, and the API serves the front end from the same address. |
| Sign-in | A cookie, with two roles | The front end and API are on the same address, so a cookie is simpler than handling tokens in the browser. It's HttpOnly, so page scripts can't read it, and SameSite=Strict, so the browser won't send it with requests that start on another website. |
| Tests | xUnit, with `WebApplicationFactory` for the API | Domain tests are plain and fast. API tests run the real app, with its middleware and a real SQLite database, so they catch what mocks would miss. |
| CI | GitHub Actions | Every pull request runs the API tests, the front-end type check and build, and a Docker build. |

The projects started from the standard templates: `dotnet new web` for the API, `dotnet new xunit` for its tests, and `sv create` with Prettier, Tailwind and the static adapter for the front end.

Design decisions:

- **The business rules live in the domain classes.** A job changes only through its own methods, which throw when a rule is broken. Endpoints load the job, call one method and save. One table lists which status can move to which, so a bike can't be marked collected before it's ready. The front end shows its buttons from a copy of that table, and the API still checks every change.
- **Endpoints are grouped by feature** (`Features/WorkOrders`, `Features/Auth` and so on) as Minimal API handlers with typed results, not controllers.
- **One error rule.** Every error is a problem-details JSON response: 400 for invalid input, checked once at the API with data annotations; 401 when not signed in; 403 when not allowed; 404 for an unknown job; 422 for a broken business rule; and 500, with a trace ID but no internals, for anything unexpected. The front end shows the API's message as it is.
- **Money is stored in cents and time in minutes,** as whole numbers. This avoids rounding problems, and SQLite has no decimal type anyway.
- **Each job keeps a copy of the labour rate,** so changing the shop rate later doesn't change old jobs.
- **Times are stored in UTC and promised dates as plain dates.** The clock is injected (`TimeProvider`), so tests and the demo data can set the time.
- **The front end loads data in SvelteKit `load` functions** and reloads it after every change, so each page shows what the API saved. There's no client-side store to keep in sync.
- **Passwords are hashed with ASP.NET Core Identity's password hasher,** without the rest of the Identity.
- **The database migrates itself on startup.** Demo mode, an environment variable that Docker Compose turns on, fills an empty shop with sample jobs through the same domain methods the API uses.

Things I chose not to use: a repository layer on top of EF Core (its DbContext already does that job), CQRS or MediatR, microservices, a separate database server, and a front-end state library. The app is too small to need any of them.

## 5. Trade-offs and limitations

- **SQLite only allows one write at a time, on one machine.** A few staff saving forms won't come close to that limit. I'd switch to PostgreSQL if the app ever needed to run as more than one copy, for example a central server for several shops.
- **Backups aren't automated.** Everything the app saves is in one data folder in a Docker volume. A backup is a copy of that folder, taken while the app is stopped.
- **Two people editing the same job: the last save wins.** Nothing checks that the job hasn't changed since it was opened. With a few staff that's rare. A version number on each job, with a "this job has changed" message, is the fix.
- **A job keeps only its current status details.** Reopening a collected job clears its receipt number, so the owner looks the sale up in the POS. A status history would keep it.
- **Notes can't be edited or deleted.** They're the job's history, for example what the customer agreed to by phone, so a mistake is fixed with a new note.
- **The job board loads every open job and filters in the browser.** That's quick for the few dozen bikes a shop has in at once. It doesn't refresh itself, so someone else's change shows when the page next loads. Hundreds of open jobs would need filtering and paging on the server.
- **The dashboard is worked out in the browser** from the same open jobs. Showing history, such as labour overruns over the last month, or anything staff shouldn't see would move it to its own API endpoint, checked on the server. Its "waiting" days count from the job's last status change, so a job reopened after a mistaken collection counts from the reopen.
- **"Overdue" uses the browser's date.** The API runs in UTC in Docker, and the shop's time zone isn't a setting yet, so the date comes from the shop's PC.
- **There's no bike record.** A bike is described on each job, so its history is found through its customer, and it doesn't follow the bike to a new owner.
- **Phone search understands North American numbers.** It compares digits and drops a leading 1. Other numbers are still found by typing their digits the same way.
- **Staff accounts are seeded,** with no screen to manage them yet (see next steps). The deactivated-account message says to ask the owner to reactivate it, which the owner can't yet do in the app.
- **Sign-in security is sized for one shop on its own network.**
  - A deactivated user can't sign in again, but a session they already have open lasts until it expires, at most 8 hours (a working day) after they signed in.
  - The keys that protect the sign-in cookie are stored unencrypted in the data folder, and ASP.NET Core warns about this at first start. Anyone who could read that folder could fake a sign-in, but the database is in the same folder, so they could already read everything.
  - There's no limit on sign-in attempts. That should be added before the app is ever on the internet.
  - When a session expires, a button that calls the API shows a plain "Unauthorized", and anything typed is lost on signing in again.
- **The API relies on the forms for a few limits.** Money and time have no upper limit, and the staff picked for a job or a labour entry aren't checked against real accounts. A bad value sent straight to the API gets a 500 instead of a 400 or 422. The forms cap the amounts and pick staff from a list.
- **Going over the estimate shows a warning when the part is added, but doesn't block it.** For something small, like a $3 cable, the mechanic can add it to the bill and carry on. For anything bigger, they put the job on hold and call the customer first. Blocking every overage would hold up the workshop for small parts.
- **When the owner corrects a closed job, the POS sale isn't updated.** The till has to be adjusted separately.
- **Demo data is seeded at startup.** The demo accounts, with one known password, are created in every mode, since nobody could sign in otherwise. Sample jobs are added only in demo mode and only to an empty shop, with times worked out from when the app first starts.
- **Migrations run at startup.** That's simple for one copy of the app. With several, migrating would be a separate deployment step.
- **The front end has no tests of its own.** `svelte-check` catches type errors in CI, and I tested every screen by hand. Its API types are written by hand, so a change to the API's responses has to be copied over.

## 6. Next steps

What I'd build and improve with more time, in this order:

1. **User admin.** Today the accounts are seeded. The owner would create staff accounts, deactivate and reactivate them, reset passwords and change roles, with a guard so the last owner can't be removed.
2. **Linked jobs.** A new job could be linked to the same customer's earlier jobs, for example a comeback when our work needs redoing, or finishing work that was left undone. Comebacks would then show on the dashboard as a quality signal.
3. **A bike record,** so a bike's history follows the bike, not just its customer.
4. **A browser test of the main flow,** from check-in to collected, running in CI with the API tests.
5. **A status history on each job,** showing who changed what and when, which would also keep a reopened job's receipt number.
6. **Editing customer details.**
7. **A reminder when a job is marked ready with no time logged.**
8. **More on the dashboard:** jobs stuck on hold, and estimated against actual hours.

Before the app runs anywhere but the shop's own network:

- **Limit repeated sign-in attempts.**
- **Sign a deactivated user out straight away,** instead of when their session ends.

As the app grows:

- **Move the check on who can edit a job into ASP.NET Core's resource-based authorisation,** once there's more than one rule like it.
- **Pop-up notifications for quick success messages,** alongside the error banner.

## Testing approach

For the main rules (status changes, money, permissions), the tests are written before the code, and both go in the same commit. API integration tests run the real app against a real SQLite database and cover sign-in, permissions, validation and error responses. The front end is checked by `svelte-check` in CI and by hand; there's no browser test yet (see next steps).

## How I used AI tools

I used Claude Code as a pair programmer, and I led the work. I chose the problem and the stack, set the scope and priorities, and made the product and design decisions. Claude Code wrote most of the first drafts of the code and tests to my direction. I reviewed, reworked and approved every change before it was committed, one small commit at a time.

- **Domain knowledge:** the assumptions and the workflow come from my years at a marine repair shop: how jobs were tracked, where the owner lost time, and what customers were promised.
- **Starting point:** I based the project skeleton (sign-in, error handling, Docker and CI) on a personal ASP.NET and Svelte project of mine, so the time went into the shop's features.

### Decisions I made and problems I caught

- **Redesigned the job lifecycle so mistakes can be recovered.** The first domain model made Collected and Cancelled permanent, so one misclick would lock a job in the wrong state for good. I reworked the state machine: closing a job now requires a confirmation with the POS receipt number or a cancellation reason, and only the owner can correct or reopen a closed job. The API enforces both rules, not just the UI.
- **Based the product decisions on how a real repair shop works.** From my years at a marine repair shop, I designed the workflow around how counter staff and mechanics actually work:
  - The estimate is the customer's agreement, so there's no separate "approved amount" to keep in sync.
  - A part that goes over the estimate warns instead of blocking, so a $3 cable doesn't stop the workshop.
  - Notes are permanent, giving every job a record of what the customer agreed to.
  - The whole team sees the dashboard, because in a small shop everyone helps clear late jobs.
- **Managed scope against a fixed deadline.** I split the work into must-have, nice-to-have and hardening tiers, built the core flow end to end first, and cut linked jobs and user admin rather than ship anything half-finished. I also removed over-engineering a three-person shop didn't need, such as constant-time sign-in checks.
- **Set the testing strategy.** Each business rule gets one test, input is validated once at the API boundary, and the API tests run the real app against a real database. I cut an early generated suite of 63 cases, many of them repeating the API's validation, down to a focused set that still covers every rule and stays quick to change.
- **Tested the app as its users would.** I worked through every screen as the owner and as staff, and ran the Docker build myself. That caught a navigation bug that type checks and API tests couldn't: a job opened from the dashboard sent the user back to the job board.
- **Set and enforced the code standards.** The code follows my conventions: my own `.editorconfig` formatting rules, explicit types, intention-revealing names, small single-purpose functions, and comments only where they explain intent. I reviewed every commit against them.

## Time spent

About 24 hours over eight days, alongside other commitments.

| Day | Hours | What got done |
| --- | --- | --- |
| Fri 2 Oct | 1 | Read the brief, picked the problem, planned the scope |
| Sat 3 Oct | 2 | Planning, and the project skeleton from my own project |
| Sun 4 Oct | 3.5 | Reviewed the skeleton, sign-in and the first tests |
| Mon 5 Oct | 3 | Job statuses, check-in and closed jobs in the domain |
| Tue 6 Oct | 5 | Database, API endpoints and their tests |
| Wed 7 Oct | 3 | Job board, check-in and the job page |
| Thu 8 Oct | 3 | Labour, parts and notes, the dashboard and the demo data |
| Fri 9 Oct | 3 | Dashboard links, the README and this document |
