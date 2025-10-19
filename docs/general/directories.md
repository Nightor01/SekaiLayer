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

## Asset Groups

Each asset group may contain tilesets or entity images.
Such files may be copied and imported using
Sekai-Layer or be copied manually and later on imported
into Sekai-Layer. What each file represents is included
inside the `config.json` file that each asset group has.

### Entity Images

Represent entities placeable into worlds.

### Tilesets

Images that contain individual tiles – the small chunks that
worlds are built from in Sekai-Layer.

## Worlds

Worlds may contain individual layers of a world, entities,
placeholders with notes and many other things.