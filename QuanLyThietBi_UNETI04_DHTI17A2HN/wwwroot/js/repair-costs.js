document.addEventListener("DOMContentLoaded", () => {
    const status = document.getElementById("TrangThaiMoi");
    const fields = document.getElementById("costBreakdown");
    const preview = document.getElementById("costPreview");
    if (!status || !fields || !preview) return;
    const output = document.getElementById("calculatedCost");
    const ids = ["TienCong", "ChiPhiKhac", "GiamTru"];
    function update() {
        const completing = status.value === "Hoàn thành";
        fields.hidden = !completing;
        fields.disabled = !completing;
        preview.hidden = !completing;
        const values = ids.map(id => {
            const input = document.getElementById(id);
            input.required = completing;
            return input.value === "" ? NaN : Number(input.value);
        });
        document.getElementById("NoiDungChiPhiKhac").required = completing && values[1] > 0;
        document.getElementById("LyDoGiamTru").required = completing && values[2] > 0;
        const total = Number(preview.dataset.parts) + values[0] + values[1] - values[2];
        const valid = values.every(v => Number.isFinite(v) && v >= 0 && v <= 1e12) && total >= 0;
        output.textContent = valid ? total.toLocaleString("vi-VN", { maximumFractionDigits: 2 }) + " VNĐ" : "Vui lòng kiểm tra các khoản chi phí / giảm trừ";
    }
    status.addEventListener("change", update);
    fields.addEventListener("input", update);
    update();
});
