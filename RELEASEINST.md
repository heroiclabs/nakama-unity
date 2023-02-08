Release Instructions
===

These instructions guide the release process for new official Nakama client SDK build and releases to the [Unity Asset Store](https://assetstore.unity.com/).

## Steps
1. Ensure the Unity repository has an up-to-date version of an official Nakama .NET release: https://www.nuget.org/packages/NakamaClient/

2. Update the version number in this repository's package.json file.

3. Update and tidy up the CHANGELOG.

4. Create the release commit and tag it.

```shell
git add CHANGELOG
git commit -m "Nakama Unity 2.5.0 release."
git tag -a v2.5.0 -m "v2.5.0"
git push origin v2.5.0 master
```

5. Create the .unitypackage distribution by running the below script from the repository root. Note, we temporarily move the Packages folder under the Assets folder because Unity can only export imported assets, i.e., files underneath the `Assets` folder.

`
./export-unitypackage.sh $UNITY_2019_4_40f1_EXECUTABLE
`

6. Create a release on GitHub: https://github.com/heroiclabs/nakama-unity/releases/new and attach `Nakama.unitypackage` to the release.

7. Login to the Unity Asset Store publisher dashboard: https://publisher.unity.com/packages

8. Click on the button that says "Create new draft to edit package" to begin a submission for a new package version.

9. Update the fields and forms in the new submission. Inside the `Version changes` textarea, concatenate the changelog from the Unity client onto the .NET client so all changes are readily visible to Asset Store
users.

10. Back in your Unity client, click on `Asset Store Tools -> Package Upload`. Enter your credentials.

11. Upon logging in you should see a Nakama package entry with a `Draft` label next to it. Tick the `include dependencies` and click on the `Upload` button. You should see a success dialog.

12. Back in the web dashboard, click on the `Edit` button in section 3 and answer the questions: (a) We support all platforms. (b) We support 2018.4 onwards (c) We are compatible with all render pipelines (d) Our package does not require any other Asset Store packages as dependencies.

13. Click on the `Preview in Asset Store` button to confirm everything looks okay.

14. Click on the submit button. You're done!