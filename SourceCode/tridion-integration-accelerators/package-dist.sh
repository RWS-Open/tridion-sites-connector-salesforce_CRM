#!/usr/bin/env bash
# Package the .NET Accelerator modules to a distribution
#
PACKAGE_DIR='integration-accelerators-'$(date "+%Y.%m.%d")
mkdir $PACKAGE_DIR
mkdir $PACKAGE_DIR/Areas
mkdir $PACKAGE_DIR/bin
mkdir $PACKAGE_DIR/cms

# TIF
#
./sign-assembly.sh Common/dotnet/Sdl.Dxa.Integration.Client/bin/Debug/Tridion.ConnectorFramework.Contracts.dll
./sign-assembly.sh Common/dotnet/Sdl.Dxa.Integration.Client/bin/Debug/Tridion.ConnectorFramework.Connector.SDK.dll
./sign-assembly.sh Common/dotnet/Sdl.Dxa.Integration.Client/bin/Debug/Tridion.Remoting.Contracts.dll
cp Common/dotnet/Sdl.Dxa.Integration.Client/bin/Debug/Tridion.ConnectorFramework.*.dll "$PACKAGE_DIR"/bin
cp Common/dotnet/Sdl.Dxa.Integration.Client/bin/Debug/Tridion.Remoting.Contracts.dll "$PACKAGE_DIR"/bin

# Accelerator Common
#
echo "Copying Common Content Porter package..."
cp Common/cms/IntegrationForm-v1.1.0.zip "$PACKAGE_DIR"/cms
echo "Copying Common libs..."
cp Common/dotnet/Sdl.Dxa.Integration.Client/bin/Debug/Sdl.Dxa.Integration.Client.dll "$PACKAGE_DIR"/bin
cp Common/dotnet/Sdl.Dxa.Integration.Client/bin/Debug/Newtonsoft.Json.dll "$PACKAGE_DIR"/bin
cp Common/dotnet/Sdl.Dxa.Integration.Form/bin/Sdl.Dxa.Integration.Form.dll "$PACKAGE_DIR"/bin
cp Common/dotnet/Sdl.Dxa.Integration.Personalization/bin/Sdl.Dxa.Integration.Personalization.dll "$PACKAGE_DIR"/bin
echo "Synching Common Areas..."
rsync -ua --progress --exclude="._*" Common/dotnet/Sdl.Dxa.Integration.Form/Areas/IntegrationForm "$PACKAGE_DIR"/Areas

# CRM Accelerator
#
#echo "Copying CRM Content Porter package..."
#cp CRM/cms/CRM-v1.0.0.zip "$PACKAGE_DIR"/cms
echo "Copying CRM libs..."
cp CRM/dotnet/Sdl.Dxa.Modules.Crm/bin/Sdl.Dxa.Modules.Crm.dll "$PACKAGE_DIR"/bin
echo "Syncing CRM Areas..."
rsync -ua --progress --exclude="._*" CRM/dotnet/Sdl.Dxa.Modules.Crm/Areas/CRM "$PACKAGE_DIR"/Areas

# ECommerce Accelerator
#
echo "Copying E-Commerce Content Porter package..."
cp ECommerce/cms/ECommerce-v1.0.0.zip "$PACKAGE_DIR"/cms
echo "Copying ECommerce libs..."
cp ECommerce/dotnet/Sdl.Dxa.Modules.ECommerce/bin/Sdl.Dxa.Modules.ECommerce.dll "$PACKAGE_DIR"/bin
echo "Syncing ECommerce Areas..."
rsync -ua --progress --exclude="._*" ECommerce/dotnet/Sdl.Dxa.Modules.ECommerce/Areas/ECommerce "$PACKAGE_DIR"/Areas

# External Content Accelerator
#
echo "Copying ExternalContent Content Porter package..."
cp ExternalContent/cms/ExternalContent-v1.1.0.zip "$PACKAGE_DIR"/cms
echo "Copying ExternalContent libs..."
cp ExternalContent/dotnet/Sdl.Dxa.Modules.ExternalContent/bin/Sdl.Dxa.Modules.ExternalContent.dll "$PACKAGE_DIR"/bin
cp ExternalContent/dotnet/Sdl.Dxa.Modules.ExternalContent/bin/HttpMultipartParser.dll "$PACKAGE_DIR"/bin
cp ExternalContent/dotnet/Sdl.Dxa.Modules.ExternalContent/bin/Microsoft.IO.RecyclableMemoryStream.dll "$PACKAGE_DIR"/bin
cp ExternalContent/dotnet/Sdl.Dxa.Modules.ExternalContent/bin/System.Buffers.dll "$PACKAGE_DIR"/bin

echo "Syncing ExternalContent Areas..."
rsync -ua --progress --exclude="._*" ExternalContent/dotnet/Sdl.Dxa.Modules.ExternalContent/Areas/ExternalContent "$PACKAGE_DIR"/Areas

cd $PACKAGE_DIR
zip -r ../"$PACKAGE_DIR".zip *
cd ..
rm -r $PACKAGE_DIR
