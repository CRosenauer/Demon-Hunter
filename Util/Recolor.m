function Recolor(inPath, outPath)
    inColor =  [ 15, 15, 101; 56, 55, 188; 86, 179, 192; 16, 92, 104; 224, 112, 178; 132, 35, 92; 64, 4, 38 ];
    outColor = [ 127, 12, 19; 186, 0, 9;   255, 77, 38;  155, 38, 32; 248, 228, 17;  132, 130, 0; 63, 63, 0 ];

    listings = dir(inPath);
    fileCount = length(listings);
    % nameList = strings(fileCount - 2, 1);

    for ii = 3:fileCount
        filename = listings(ii).name;
        [fPath, fName, fExt] = fileparts(filename);

        if(fExt == ".meta")
            continue;
        end

        [image, map, alpha] = imread(inPath + "/" + fName + fExt);
        out = ReplaceColor(image, inColor, outColor);
        imwrite(out, outPath + "/" + fName + fExt, 'Alpha', alpha);
    end
end

function out = ReplaceColor(image, inColor, outColor)
    [l, w, d] = size(image);

    out = image;

    colorCount = size(inColor);

    for ii = 1:l
        for jj = 1:w
            for kk = 1:colorCount(1)
                colorValue = image(ii, jj, :);
                color = inColor(kk, :);

                colorMatch = true;

                for ll = 1:3
                    if colorValue(ll) ~= color(ll)
                        colorMatch = false;
                    end
                end

                if colorMatch
                    for ll = 1:3
                        out(ii, jj, ll) = outColor(kk, ll);
                    end
                    break;
                end
            end
        end
    end
end