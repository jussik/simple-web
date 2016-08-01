import * as React from "react";
import {Dispatch} from "redux";
import {connect} from "react-redux";

import {removeProduct} from "../store/products";
import {IProduct, Products} from "../api/products";

type ActionProp<T> = (payload: T) => ReduxActions.Action<T>;

interface IProductProps {
    product: IProduct;
}
interface IProductDispatchProps {
    onRemove: ActionProp<IProduct>;
}
class Product extends React.Component<IProductProps & IProductDispatchProps, {}> {
    render() {
        const onRemove = this.props.onRemove.bind(this, this.props.product);
        return <div>
            <span>{this.props.product.name} </span>
            <button onClick={onRemove}>&times;</button>
        </div>;
    }
}

const mapDispatch = (dispatch: Dispatch<any>) => ({
    onRemove: (p: IProduct): any =>
        Products.remove(p).then(() => dispatch(removeProduct(p)))
});
export default connect<{}, IProductDispatchProps, IProductProps>(null, mapDispatch)(Product);