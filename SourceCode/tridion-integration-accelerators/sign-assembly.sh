echo "Signing $1..."
curl -X POST 'http://assemblysigner.tridion.global.sdl.corp/GetSigned?file=$1' --fail --data-binary "@$1" -o "$1" -H 'Authorization: Basic Z2xvYmFsXHNydi1jbWJ1aWxkOnNydl90cmlkaW9uX2Nt' -H 'cache-control: no-cache'
