# Social Environment Autonomous Navigation 2.0

Documentation: [sean.interactive-machines.com](https://sean.interactive-machines.com)

This is the [Unity](https://unity.com) project for SEAN: Social Environment for Autonomous Navigation

## Source Repositories

Other repositories for the project are:

  - Catkin Workspace: https://github.com/yale-sean/sim_ws

  - ROS project: https://github.com/yale-sean/social_sim_ros

  - Unity Project: https://github.com/yale-sean/social_sim_unity

  - Documentation: https://github.com/yale-sean/social_sim_docs


## Developer Information

### Generate documentation

```
cp README.md Documentation/index.md
docfx Documentation/docfx.json --force
```

Copy the documentation into the web documentation project.

First, set the web project directory:

```
export WEB_DIR=$HOME/src/yale/social_sim_docs
```

Then copy the `api` folder:

```
mkdir -p $WEB_DIR/static/api/unity
cp -r _site/api/* $WEB_DIR/static/api/unity/
cp -r _site/styles $WEB_DIR/static/api/
```


### Code Formatting and Linting

Try to keep the typical C# code style enforced by Visual Studio.

In case some files get committed or edited and no longer adhere to this style, the whole project can be re-styled using:

```
dotnet tool install -g dotnet-format
dotnet format social_sim_unity.sln
```
