#!/bin/bash

# Copyright 2023 The Nakama Authors
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
# http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

# first open unity and force upm to import all dependencies
$1 -batchmode -quit -nographics -projectPath .
# move package to assets folder because that's the only location Unity can export a package from
mv ./Packages ./Assets
$1 -noUpm -batchmode -quit -nographics -projectPath . -exportPackage Assets/Packages/Nakama ./Nakama.unitypackage
# move the package back
mv ./Assets/Packages ./

# remove .meta files generated from shuffling around the package
rm Packages/Nakama.meta
rm Packages/manifest.json.meta
rm Packages/packages-lock.json.meta

# reimport with Unity to clean up any stale import data e.g., EditorBuildSettings.asset
$1 -batchmode -quit -nographics -projectPath .
