## Command to deploy:

$env:GHCR_PAT="ghp_xxxxxxxxx"

az deployment sub create `
  --location centralus `
  --template-file main.bicep `
  --parameters resourceGroupName='jpsd-test-rg' `
  --parameters dbAdminUsername='your-admin-db-name' `
  --parameters dbAdminPassword='your-admin-db-password' `
  --parameters ghcrPat=$env:GHCR_PAT `
  --parameters location=centralus