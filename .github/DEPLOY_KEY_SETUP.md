# Deploy Key Setup for Code Formatting Workflow

This repository uses a deploy key to allow the code formatting workflow to push formatted code even when branch protection rules are enabled.

## Setup Instructions

### 1. Generate an SSH Key Pair

```bash
ssh-keygen -t ed25519 -C "github-actions-format@scopestify" -f deploy_key -N ""
```

This creates:
- `deploy_key` (private key)
- `deploy_key.pub` (public key)

### 2. Add the Deploy Key to GitHub Repository

1. Go to your repository settings: `https://github.com/gwennlbh/scopestify/settings/keys`
2. Click "Add deploy key"
3. Title: `Format workflow deploy key`
4. Key: Paste the contents of `deploy_key.pub`
5. ✅ Check "Allow write access"
6. Click "Add key"

### 3. Add the Private Key as a Repository Secret

1. Go to repository secrets: `https://github.com/gwennlbh/scopestify/settings/secrets/actions`
2. Click "New repository secret"
3. Name: `DEPLOY_KEY`
4. Secret: Paste the contents of `deploy_key` (the private key file)
5. Click "Add secret"

### 4. Clean Up Local Keys

```bash
rm deploy_key deploy_key.pub
```

## Why This Is Needed

When branch protection rules are enabled with required status checks:
- The default `GITHUB_TOKEN` cannot push to protected branches until all checks pass
- This creates a chicken-and-egg problem for formatting workflows
- Deploy keys bypass these restrictions and can push even when checks are pending

## Security Notes

- The deploy key only has access to this specific repository
- It's configured with minimal permissions needed (read + write to repository)
- The private key is stored securely as an encrypted GitHub secret
- The workflow only uses the key for pushing formatting changes to the main branch