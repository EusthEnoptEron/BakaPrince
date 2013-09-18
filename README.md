BakaPrince
==========

C# application that uses PrinceXML to generate PDFs from Baka-Tsuki's projects. Feed it a JSON configuration file and it will create a nice PDF for you.


## Usage
```
Usage: bakaprince [OPTIONS]+ config-path
Create a PDF from a Baka-Tsuki project.

Options:
  -p, --prince=PATH          the PATH where PrinceXML is located. Leave away
                               to find it automatically.
  -o                         where to write the resulting PDF
  -h, --help                 show this message and exit
  -c, --cache                enable caching
```

## JSON format
### Keys
#### title
Set the `title` key to define what the name of the novel is. (Will be applied to the PDF config)

#### defaults
Set the default config for your pages.

* `name`: Wiki-title of the chapter (not useful as a default value)
* `title`: Text that will be displayed for the chapter [`name`]
* `prefix`: String that should be prepended to `name`. E.g. "Volume 1 "
* `wiki`: Url to the wiki where the pages are [http://www.baka-tsuki.org/project/]
* `notitle`: Whether or not the script should hold off making a visible title for the chapter (the header will still be created) [false]
* `noheader`: Whether or not to hide the header [false]
* `pagebreak`: Whether or not there should be a pagebreak before the chapter [true]
* `entrypicture`: Whether or not the first picture should be placed before the title [false]

#### images
Array of image urls. Relative paths are resolved relatively to the JSON file.

#### pages
List of chapter definitions. Give a string to use that as the name and default values for everything else, or make an object with the keys you see in the `defaults` section.

#### translators
List of contributing translators.

#### editors
List of contributing editors.

#### authors
Writer(s) involved in the light novel at hand.

#### artists
Artist(s) involved in the light novel at hand.
