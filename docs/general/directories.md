# Program Directory

The program directory (the directory where the executable
is placed) contains files important for execution,
configuration files, and other assets or resources.

# Vault Directories

The vault directories are such directories where all files
linked to a specific vault are placed. A general structure is
shown here:

- Root Vault Directory
  - Config
    - `config.json`
  - Assets
    - gp1
      - `config.json`
  - Worlds
    - world1
      - `config.json`
      - `included.json`

where `gp1` and `world1` are example directories that may be
found inside the directory structure.