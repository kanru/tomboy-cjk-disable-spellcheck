all: CJKDisableSpellAddin.cs CJKDisableSpellAddin.addin.xml
	gmcs CJKDisableSpellAddin.cs -resource:CJKDisableSpellAddin.addin.xml -r:/usr/lib/tomboy/Tomboy.exe -out:CJKDisableSpellAddin.dll -pkg:gtk-sharp-2.0 -target:library
