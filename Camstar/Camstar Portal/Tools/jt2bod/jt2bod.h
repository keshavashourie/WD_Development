
// COPYRIGHT 2016  

#ifndef JT2BOD_H
#define JT2BOD_H

#include <string>
#include <ostream>

#ifdef _WIN32
#define SOEXPORT _declspec(dllexport) // windows dllexport
#else
#define SOEXPORT
#endif


class CJt2Bod
{
public:
	/// <summary>Constructor.  NOTE: this class will only convert one Jt file per object instance</summary>
	/// <param name="inputFile">Path to the Jt file</param>
	SOEXPORT CJt2Bod(const char* inputFile);
	SOEXPORT ~CJt2Bod();

	/// <summary>Convert the JT file to file storage.  NOTE: only one conversion call is allowed per Jt2Bod object</summary>
	/// <param name="outputDir">Path to the output directory</param>
	/// <returns>true on success</returns>
	SOEXPORT bool ConvertToFiles(const char* outputDir);

	/// <summary>Convert the JT file to JSON strings.
	/// NOTE: only one conversion call is allowed per Jt2Bod object</summary>
	/// <param name="psLength">if psPtr is NULL, the length of the internal product structure string is returned.
	/// if psPtr is valid, psLength indicates the number of characters to copy into psPtr from the internal string.</param>
	/// <param name="pmiLength">if pmiPtr is NULL, the length of the internal PMI string is returned.
	/// if pmiPtr is valid, pmiLength indicates the number of characters to copy into pmiPtr from the internal string.</param>
	/// <param name="propLength">if propPtr is NULL, the length of the internal properties string is returned.
	/// if propPtr is valid, propLength indicates the number of characters to copy into propPtr from the internal string.</param>
	/// <param name="psPtr">the address of the allocated character array to be populated by the internal product structure string.</param>
	/// <param name="pmiPtr">the address of the allocated character array to be populated by the internal PMI string.</param>
	/// <param name="propPtr">the address of the allocated character array to be populated by the internal properties string.</param>
	/// <returns>true on success</returns>
	SOEXPORT bool ConvertToStrings(int& psLength, int& pmiLength, int& propLength,
			char** psPtr=NULL, char** pmiPtr=NULL, char** propPtr=NULL);

	/// <summary>Set log level verbosity.  Default: 0 (quiet)
	/// NOTE: this must be called prior to ConvertToFiles or ConvertToStrings</summary>
	/// <param name="val">Log level 0:default, 1:info, 2:more, 3:verbose, 4:debug</param>
	SOEXPORT void SetLogLevel(int val);
	/// <summary>Set to true to enable verbose logging.  Default: false
	/// NOTE: this must be called prior to ConvertToFiles or ConvertToStrings</summary>
	/// <param name="val">The desired value</param>
	SOEXPORT void SetVerbose(bool val);
	/// <summary>Set to true to enable XT BREP parsing.  Default: false
	/// NOTE: this must be called prior to ConvertToFiles or ConvertToStrings</summary>
	/// <param name="val">The desired value</param>
	SOEXPORT void SetLoadXt(bool val);
	/// <summary>Set to true to enable PMI parsing.  Default: true
	/// NOTE: this must be called prior to ConvertToFiles or ConvertToStrings</summary>
	/// <param name="val">The desired value</param>
	SOEXPORT void SetLoadPMI(bool val);
	/// <summary>Set to true to enable pooling which reduces BOD filesize.  Default: true
	/// NOTE: this must be called prior to ConvertToFiles or ConvertToStrings</summary>
	/// <param name="val">The desired value</param>
	SOEXPORT void SetUsePooling(bool val);
	/// <summary>Set to true to node names for file names.  Be aware that using node names
	/// will assume the node names are UNIQUE!  Default: false
	/// NOTE: this must be called prior to ConvertToFiles or ConvertToStrings</summary>
	/// <param name="val">The desired value</param>
	SOEXPORT void SetUseNodeNames(bool val);
	/// <summary>Set to true to NOT pool materials per model.  i.e materials.bod will not be created.
	/// Default: false (unless converting a single part or an assembly with a single child part
	/// NOTE: this must be called prior to ConvertToFiles or ConvertToStrings</summary>
	/// <param name="val">The desired value</param>
	SOEXPORT void SetUseLocalMaterials(bool val);
	/// <summary>In addition to writing to the console, the converter can write the supplied output stream</summary>
	/// <param name="val">The output stream to write to</param>
	SOEXPORT void SetLogStream(std::ostream* stream);
	/// <summary>Set the desired LOD level.</summary>
	/// <param name="val">The desired LOD level</param>
	SOEXPORT void SetLodLevel(int val);

	/// <summary>This will return the major/minor version of this converter.
	/// NOTE: the major version coincides with the version of BOD being produced.</summary>
	/// <param name="major">returns the major version</param>
	/// <param name="minor">returns the minor version</param>
	SOEXPORT static void GetBODVersion(int& major, int& minor);

	/// <summary>This MUST be called once at the very beginning of using this library.</summary>
	/// <param name="isJT2Go">true for JT2Go; false for TcVis.  Default: true</param>
	SOEXPORT static void init(bool isJT2Go=true);
	/// <summary>This MUST be called once at the very end of using this library.</summary>
	SOEXPORT static void fini();
};

#endif // JT2BOD_H
