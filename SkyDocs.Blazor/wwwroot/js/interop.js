//DrawImage
window.Snap = async (src, format) => {
    let rte = document.getElementById(src).children[1];

    let canvas = await html2canvas(rte, {
        scale: 0.3,
        width: 910,
        height: 500});
    let dataUrl = canvas.toDataURL(format);
    return dataUrl.split(',')[1];
}
