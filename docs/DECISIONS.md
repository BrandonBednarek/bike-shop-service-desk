# Decision document

> Work in progress. Sections marked *(to finalise)* get filled in as the build goes.

## Summary

- **The brief:** a local bike shop says "we need an app for our shop", and the owner isn't available to answer questions.
- **The problem I picked:** the repair side of the shop. I based this on a marine repair shop I worked at for several years as a teenager. Repairs there were tracked in a spreadsheet that only the owner updated. Nothing was badly broken, but it could have been organised much better, with fewer headaches and less of the owner's time.
- **What I'm building:** Bike Shop Service Desk, an internal web app that tracks each repair from check-in to pickup, plus a dashboard for the owner.
- **Who uses it:** the owner (sees everything, manages staff accounts) and staff (check bikes in, do the work, hand them back).
- **What I'm not building:** payments, invoicing, stock and anything customer-facing. The shop most likely has a POS for sales already (#2), so the app just records the POS receipt number when a bike is collected.
- **Stack:** SvelteKit front end, ASP.NET Core 10 API, SQLite, all in one Docker image that starts with one command.

## 1. Assumptions

| # | Assumption | Why | If it's wrong |
|---|---|---|---|
| 1 | A small single-location shop: the owner plus 1 to 3 staff, who all work both the counter and the workshop. | The brief says "local, independent" and the owner is busy, so it's most likely a small team where everyone does a bit of everything. | More staff or more locations would need separate counter and mechanic roles, and a location on each job. |
| 2 | The shop already uses a POS for sales, card payments and tax, but it doesn't track repairs. | Almost every shop has one for sales. Most don't track repairs, which would explain why the owner asked for an app. | If their POS already tracks repairs, they may not need this app at all, or only the owner dashboard. |
| 3 | Repairs are tracked today with paper tags on the bikes, a spreadsheet and memory. | That's how the marine repair shop I worked at did it: the owner kept the spreadsheet and updated it all. | If repairs are already tracked in a spreadsheet, this will give a centralised system where they can easily track current repairs and find old ones when new work is needed to be done. |
| 4 | Roughly 10 to 30 repair jobs a week, with a backlog in the spring. | That's about what a team that size can get through at an hour or two per job, and spring is the busy season. The marine shop I worked at had the same spring backlog. | If it's much busier, the job board would need paging and more filters to stay quick to use. |
| 5 | Customers drop bikes off in person. There's no online booking. | Most small shops work walk-in or by phone. | Online booking would just be another way of creating jobs on top of this. |
| 6 | Labour is estimated in hours at check-in. The customer pays the estimated labour plus the parts actually used. If extra work would push the bill past what the customer agreed to, staff call them first. If a mechanic is just slower than estimated, the shop absorbs it. | That's how the marine shop worked: extra work was only done once the customer approved it. It's also what customers expect from a quote. | If the shop bills actual hours instead, the bill is calculated in one place, so it's a small change. |
| 7 | Customers pay when they pick the bike up. | Payment goes through the POS, so a finished bike that hasn't been paid for is just a bike that hasn't been picked up yet. | Business customers who pay on account would need invoicing, which I've left out. |
| 8 | Staff share two desktop PCs, one at the counter and one in the workshop. Everyone has their own account. | The shop probably has these PCs already, and personal accounts show who did what. Labour can be logged against any mechanic, whoever is signed in. | A shared tablet in the workshop would need a touch-friendly layout and quick user switching. |
| 9 | Staff write the job number on the paper tag that goes on the bike. | Something physical on the bike has to point to its record. | Staff would have to find a bike's job by customer name and the bike's description, which is slower. |
| 10 | One time zone (Eastern), prices in Canadian dollars, amounts before HST. | It's a local shop, and the POS adds tax at the till. | Other regions would need time zone and tax settings. |
| 11 | Staff can see job prices. The dashboard (late jobs, time overruns) is for the owner only. | Staff talk prices with customers every day. The dashboard is for the owner to manage the shop, so staff don't need it. | Letting staff see it is a small change: allow staff on the dashboard's API and show them the link. |

## 2. Problem selection

### Who has what problem

| User | Problem today |
|---|---|
| Owner | Is the only one updating the spreadsheet, so keeping it current takes a lot of their time, and it's hard to see at a glance what's late or waiting to be picked up. |
| Staff at the counter | To answer "is my bike ready?" they have to go and check the tags. Promised dates and what the customer agreed to are on paper that gets lost. There's no history for returning customers. |
| Staff in the workshop | Not always clear what to work on next, or what the customer agreed to pay for. |
| Customers | Don't know when their bike will be ready, or whether it'll cost more than they were told. |

### Options I considered

| Option | Value to the shop | Does the POS already cover it? | Reason | Decision |
|---|---|---|---|---|
| Repair jobs, from check-in to pickup | High. Labour has no stock cost, so repairs usually make the best margin, and it's where most day-to-day problems are. | Usually not | Booking, notifications and a status page for customers all build on it. | **Chosen** |
| Online store | Medium | Often | It's a separate sales channel and doesn't help with running the shop day to day. | Not now |
| Stock / inventory | Medium | Yes | The POS already handles it. | Not needed |
| Online booking | Medium | Sometimes | It needs a job system to book into first. | Later |
| Bike rentals | Low, and not every shop rents | Rarely | Only useful to some shops, and mostly in the busy season. | Not now |

### Why repair jobs first

- It's the most valuable part of the shop that the POS doesn't already handle.
- Everything else I'd add later (booking, notifications, a status page) needs a job record to exist first.
- Counter staff, mechanics and the owner all work from the same job records. The job board shows mechanics what to do next, and the dashboard shows the owner what's late. When a customer calls to ask if their bike is ready, staff can look it up by phone number.
- The dashboard only works if staff keep their jobs up to date, so checking a bike in and logging time need to be quick.

Customers don't use the app themselves, but it still helps them. Staff get a warning when a bill goes over what the customer agreed to, so they can call and get approval before pickup. A status page for customers and "ready for pickup" messages are next steps.

### How the owner would know it's working

- Fewer "is my bike ready?" calls.
- Fewer bills that go over what the customer agreed to.
- Finished bikes spend less time waiting to be picked up.
- Estimates get closer to the time actually spent.

## 3. Scope *(to finalise)*

### Planned for the first version

- **Sign-in with two roles:** Owner and Staff. The owner manages staff accounts.
- **Check-in:** find a returning customer by phone or name, or add a new one. Record the bike, the work wanted, the estimate, the amount the customer agreed to and the promised date.
- **Job board:** every bike in the shop by status, sorted by promised date, with search and an "assigned to me" filter.
- **Job statuses:** Checked in, In progress, Ready for pickup and Collected, plus On hold (waiting for parts or for the customer) and Cancelled. Collecting a bike records the POS receipt number.
- **Work records:** labour time and parts used, a running bill, and a warning when the bill goes over the agreed amount.
- **Linked jobs:** a new job can be linked to the same customer's earlier closed jobs, for example a comeback when our work needs redoing, or finishing work that was left undone.
- **Closed jobs:** staff can't change collected or cancelled jobs. The owner can correct them but can't reopen them; a comeback becomes a new linked job.
- **Owner dashboard:** bikes in the shop, overdue jobs, jobs stuck on hold, bikes waiting to be picked up, and estimated vs actual hours.

### If there's time, in this order

1. Changing a user's role, without being able to remove the last owner.
2. Editing customer details.
3. A reminder when a job is marked ready with no time logged.
4. A history on each job, including notes like "called, left voicemail".
5. A browser test of the main flow.
6. Stopping two people from overwriting each other's changes on the same form.
7. Comebacks on the owner dashboard.
8. Generating the front end's API types from the back end instead of writing them by hand.

### Only if everything above is done

None of these are needed for a shop this size. Anything I don't get to goes in the trade-offs section.

1. Signing a deactivated user out straight away, instead of when their session ends.
2. Moving the check on who can edit a job into ASP.NET Core's built-in authorisation, instead of one plain check.
3. Pop-up notifications for quick messages, as well as the error banner.
4. Limiting repeated sign-in attempts.

### Not building, and why

| Not building | Why |
|---|---|
| Payments, invoicing and tax | The POS already does this. Doing it twice would mean two places to keep in sync. The receipt number links them. |
| Connecting to the POS or accounting software | Useful, but it depends on which systems the shop uses, and the job records need to be in place and trusted first. |
| Stock and ordering parts | That's the POS's job. Parts used are recorded on the job as simple lines. |
| Anything customer-facing | It needs a reliable job system underneath first. |
| A price list of standard services | Estimates are typed in for now. Prefilling them from a list is a good next step. |
| Phone and tablet layouts | Staff use desktop PCs (#8). |

## 4. Technical choices *(to finalise)*

| Part | Choice | Why |
|---|---|---|
| Front end | SvelteKit (single-page app), TypeScript, Tailwind CSS | I used SvelteKit on a previous project, so I can build quickly with it. A single-page app suits an internal tool that stays open all day. |
| Back end | ASP.NET Core 10 Minimal API in C# | C# is my strongest language, and Minimal APIs keep each endpoint short and easy to follow. |
| Database | SQLite with Entity Framework Core | A few users and not much data. No database server to run, and the whole database is one file. |
| Packaging | One Docker image, started with `docker compose up --build` | One command to run it, and the API serves the front end from the same address. |
| Sign-in | A cookie, with two roles | The front end and API are on the same address, so a cookie is simpler than handling tokens in the browser. It's HttpOnly, so page scripts can't read it, and SameSite=Strict, so the browser won't send it with requests that start on another website. |

The projects started from the standard templates: `dotnet new web` for the API, `dotnet new xunit` for its tests, and `sv create` with Prettier, Tailwind and the static adapter for the front end.

Some design decisions (more to come as I build):

- **The job controls its own status changes.** One table lists which status can move to which, so a bike can't be marked collected before it's ready.
- **Money is stored in cents and time in minutes,** as whole numbers. This avoids rounding problems, and SQLite has no decimal type anyway.
- **Each job keeps a copy of the labour rate,** so changing the shop rate later doesn't change old jobs.
- **Every error comes back in the same JSON format,** so the front end can handle them all the same way.

Things I chose not to use: a repository layer on top of EF Core (its DbContext already does that job), CQRS or MediatR, microservices, a separate database server, and a front-end state library. The app is too small to need any of them.

## 5. Trade-offs and limitations *(to finalise)*

- **SQLite only allows one write at a time, on one machine.** A few staff saving forms won't come close to that limit. I'd switch to PostgreSQL if the app ever needed to run as more than one copy, for example a central server for several shops.
- **Backups aren't automated.** Everything the app saves is in one data folder in a Docker volume. A backup is a copy of that folder, taken while the app is stopped.
- **The dashboard is calculated in memory.** Fine for hundreds of open jobs. With far more, the calculations would move into database queries.
- **Jobs are linked by customer, not by bike.** There's no bike record yet, so if a bike changes owners its history doesn't follow it.
- **Sign-in security is sized for one shop on its own network.**
  - A deactivated user can't sign in again, but a session they already have open lasts until it expires, at most 8 hours (a working day) after they signed in.
  - The keys that protect the sign-in cookie are stored unencrypted in the data folder, and ASP.NET Core warns about this at first start. Anyone who could read that folder could fake a sign-in, but the database is in the same folder, so they could already read everything.
  - There's no limit on sign-in attempts. That should be added before the app is ever on the internet.
- **Going over the agreed amount shows a warning but doesn't block the job.** Blocking it would hold up the workshop, so the warning just tells staff to call the customer.
- **When the owner corrects a closed job, the POS sale isn't updated.** The till has to be adjusted separately.

## 6. Next steps *(to finalise)*

Filled in at the end.

## Testing approach *(to finalise)*

For the main rules (status changes, money, linked jobs, permissions), the tests are written before the code, and both go in the same commit. API integration tests run the real app against a real SQLite database and cover sign-in, permissions, validation and error responses. If there's time, one browser test covers the main flow.

## How I used AI tools *(to finalise)*

I used Claude Code on this project. So far:

- Created a living DECISIONS document based off my lived experience with working in a marine repair shop for multiple years as a teenager. I have based every assumption through systems I've worked in or seen the owner/mechanics struggle with.
- I also have created the skeleton of this project off of the work of another full-stack ASP.NET & Svelte personal project I have been working on. This has jump started my development time and will help me spend more time on the back-end and front-end work.

## Time spent *(to finalise)*

Filled in at the end.
