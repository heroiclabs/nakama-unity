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

5. *The Unity Asset Store is not yet compatible with the new Unity Package Manager format*. For this reason, you will need to temporarily move the `Packages/Nakama` directory to `Assets/Nakama`.

6. Create the .unitypackage distribution by clicking on `Assets -> Export Package`. When prompted,
save the file as `Nakama.unitypackage` in the repository root.

7. Create a release on GitHub: https://github.com/heroiclabs/nakama-unity/releases/new and attach `Nakama.unitypackage` to the release.

8. Login to the Unity Asset Store publisher dashboard: https://publisher.unity.com/packages

9. Click on the button that says "Create new draft to edit package" to begin a submission for a new package version.

10. Update the fields and forms in the new submission. Inside the `Version changes` textarea, concatenate the changelog from the Unity client onto the .NET client so all changes are readily visible to Asset Store
users.

11. Back in your Unity client, click on `Asset Store Tools -> Package Upload`. Enter your credentials.

12. Upon logging in you should see a Nakama package entry with a `Draft` label next to it. Tick the `include dependencies` and click on the `Upload` button. You should see a success dialog.

13. Back in the web dashboard, click on the `Edit` button in section 3 and answer the questions: (a) We support all platforms. (b) We support 2018.4 onwards (c) We are compatible with all render pipelines (d) Our package does not require any other Asset Store packages as dependencies.

14. Click on the `Preview in Asset Store` button to confirm everything looks okay.

15. Click on the submit button. You're done!