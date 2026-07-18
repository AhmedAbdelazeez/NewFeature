# Task List: Tourism & Operations Integration

## Phase 1: Database Models & Schema (NewFeature)
- [x] Create `TourismHotelBooking.cs` and `TourismTour.cs` models
- [x] Create `OperationsDailyPlan.cs` and `OperationsIncident.cs` models
- [x] Create `TourismDtos.cs` and `OperationsDtos.cs` structures
- [x] Register new models in `ApplicationDbContext.cs` and set decimal precisions
- [x] Add mock seed data in `DbInitializer.cs`
- [x] Run Entity Framework migration (`AddTourismAndOperations`) and update the database

## Phase 2: Services & API Controllers (NewFeature)
- [x] Create `ITourismService.cs` & `TourismService.cs`
- [x] Create `IOperationsService.cs` & `OperationsService.cs`
- [x] Register services in `Program.cs` of Portal
- [x] Create `TourismController.cs` & `OperationsController.cs`

## Phase 3: Interactive Management Portal (NewFeature Pages)
- [x] Create Page `Tourism.cshtml` & `Tourism.cshtml.cs`
- [x] Create Page `Operations.cshtml` & `Operations.cshtml.cs`
- [x] Update navigation bar to link to the new pages in `_Layout.cshtml`

## Phase 4: Executive Dashboard Integration (project)
- [x] Update `PortalDtos.cs` with the new tourism and operations KPI structures
- [x] Update `PortalIntegrationService.cs` to fetch the KPIs from the Portal API
- [x] Update `DashboardDataController.cs` to expose `/api/dashboard-data/tourism-kpis` and `/api/dashboard-data/operations-kpis`
- [x] Update Razer sub-pages `_Tourism.cshtml` and `_Operations.cshtml` in the Dashboard project
- [x] Update `portal-integration.js` to bind the 7 KPIs for both departments
- [x] Update `app.js` with sector layouts and mappings for the new cards
- [x] Add Tourism & Operations overview cards to `_Overview.cshtml` / main page of the dashboard

## Phase 5: Verification
- [x] Build and verify no compilation errors in both projects
- [x] Ensure database tables are successfully seeded and updated
