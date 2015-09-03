# ModularSkylines
A modular API overhaul mod for Cities: Skylines

##### A rough outline of how the mod will work now:

- Buildings which utilize this mod will have a single 'CoreAI' class which inherits from CommonBuildingAI, and basically has an override for each of the important methods for service and zoned buildings. Each said override contains a delegate which is invoked when that method is called.

- 'Modules' are static classes made by me and/or any other modders flagged with an attribute. This attribute also denotes what kind of behavior the class pertains to (i.e. Light Residential, Police Station, Monument, etc). Any methods in said class which are flagged with another attribute and named correctly are found on load and a delegate created for them, along with various metadata such as if the method is 'default' (aka vanilla behavior converted into module form). A manager stores these delegates so that they only need found and created once.

- Each CoreAI contains a Properties bag of sorts, and as the CoreAI instance is passed into the delegate, the delegate functions have access to a universal, extensible collection of variables to update/modify. 

- An asset which uses this mod will include an XML file along with it's .crp, and the XML will contain the configuration data for said asset. The configuration data will include options to toggle functionality (such as LightResidential, HeavyResidential, PoliceStation, FireStation, etc.). Further, most of these will have the ability to manually configure the values (number of Educated Workers, Students, etc) as well as a checkbox which toggles the auto-gen of the same values. If the auto-gen is turned on AND values are given for the same field, the modules determine the behavior (default will be they get added together). Asset makers may also specify a default type: this is the type which will be fabricated and saved (i.e. if this mod is removed, it will become the specified type), thus also meaning this mod will not break saves.

- Which modules for a given Behavior are used at runtime is selected by the user, not the asset maker, thus while the asset maker may set his asset to use Light Residential Auto-Gen, how that auto-gen actually works will vary from user to user. Behaviors are configured by the user in the mod's options screen. Most modules can have one or more behaviors attatched to them. All default to vanilla modules which I am creating as part of this mod, and will behave about the same as vanilla AI methods would. 

##### The following are a couple features I want to incorporate, but haven't started on yet:

- Modders and users can create 'presets' of module configurations, which are saved in this mod's folder (for user configs) or in the mod folder (for modder's configs). This will save users the trouble of configuring modules, and allow other modders who release 'overhauls' to specify how their modules should ideally be configured.

- When the game is saved, this mod will spawn an additional file; a .XML file with otherwise the same name, in the same folder as the save, and save any persistent custom data to it as needed. This will allow for customization without breaking saves, though swapping around module configurations may have weird results, as different modules will store different data.

- How data is presented in-game will have it's own set of modules, and users will be able to select which module they prefer.

# License
Original code contained within: Eclipse Public License (https://www.eclipse.org/legal/epl-v10.html)

Any and all code which originated from Colossal Order remains property of Colossal Order (obviously)
